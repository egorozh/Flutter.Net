using System;
using System.Collections.Generic;
using System.Linq;
using Flutter.Foundation;

namespace Flutter.Widgets;

public sealed record RouteSettings(string? Name = null, object? Arguments = null);

public abstract class Route
{
    protected Route(RouteSettings? settings = null)
    {
        Settings = settings ?? new RouteSettings();
    }

    public RouteSettings Settings { get; }

    internal NavigatorState? Navigator { get; private set; }

    internal void Attach(NavigatorState navigator)
    {
        Navigator = navigator;
        OnAttach();
    }

    internal void Detach()
    {
        OnDetach();
        Navigator = null;
    }

    protected virtual void OnAttach()
    {
    }

    protected virtual void OnDetach()
    {
    }

    public virtual bool WillPop()
    {
        return true;
    }

    public virtual void DidPush()
    {
    }

    public virtual void DidPop(Route? previousRoute)
    {
    }

    public virtual void DidPopNext(Route nextRoute)
    {
    }

    public virtual void DidChangeNext(Route? nextRoute)
    {
    }

    public virtual void DidChangePrevious(Route? previousRoute)
    {
    }

    public virtual void Dispose()
    {
    }

    public abstract Widget BuildPage(BuildContext context);
}

public abstract class PageRoute : Route
{
    protected PageRoute(RouteSettings? settings = null) : base(settings)
    {
    }
}

public sealed class BuilderPageRoute : PageRoute
{
    private readonly Func<BuildContext, Widget> _builder;

    public BuilderPageRoute(Func<BuildContext, Widget> builder, RouteSettings? settings = null) : base(settings)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public override Widget BuildPage(BuildContext context)
    {
        return _builder(context);
    }
}

public abstract class NavigatorObserver
{
    public NavigatorState? Navigator { get; internal set; }

    public virtual void DidPush(Route route, Route? previousRoute)
    {
    }

    public virtual void DidPop(Route route, Route? previousRoute)
    {
    }

    public virtual void DidReplace(Route newRoute, Route? oldRoute)
    {
    }
}

public sealed class Navigator : StatefulWidget
{
    public Navigator(
        Route initialRoute,
        IReadOnlyList<NavigatorObserver>? observers = null,
        Key? key = null) : base(key)
    {
        InitialRoute = initialRoute ?? throw new ArgumentNullException(nameof(initialRoute));
        Observers = observers ?? [];
    }

    public Route InitialRoute { get; }

    public IReadOnlyList<NavigatorObserver> Observers { get; }

    public override State CreateState()
    {
        return new NavigatorState();
    }

    public static NavigatorState Of(BuildContext context)
    {
        return MaybeOf(context)
               ?? throw new InvalidOperationException("Navigator not found in context.");
    }

    public static NavigatorState? MaybeOf(BuildContext context)
    {
        return context.DependOnInherited<NavigatorScope>()?.Navigator;
    }

    public static bool TryHandleBackButton()
    {
        return NavigatorBackButtonDispatcher.DispatchBackButton();
    }
}

public sealed class NavigatorState : State
{
    private readonly List<Route> _history = [];
    private readonly List<NavigatorObserver> _observers = [];
    private readonly Func<bool> _backButtonHandler;

    public NavigatorState()
    {
        _backButtonHandler = HandleBackButton;
    }

    private Navigator CurrentWidget => (Navigator)Element.Widget;

    public bool CanPop => _history.Count > 1;

    public Route? CurrentRoute => _history.Count == 0 ? null : _history[^1];

    public override void InitState()
    {
        base.InitState();
        SyncObservers(Array.Empty<NavigatorObserver>(), CurrentWidget.Observers);
        PushInitialRoute();
        NavigatorBackButtonDispatcher.AddHandler(_backButtonHandler);
    }

    public override void DidUpdateWidget(StatefulWidget oldWidget)
    {
        base.DidUpdateWidget(oldWidget);
        var oldNavigator = (Navigator)oldWidget;
        SyncObservers(oldNavigator.Observers, CurrentWidget.Observers);
        if (_history.Count == 0)
        {
            PushInitialRoute();
        }
    }

    public override void Activate()
    {
        base.Activate();
        NavigatorBackButtonDispatcher.AddHandler(_backButtonHandler);
    }

    public override void Deactivate()
    {
        NavigatorBackButtonDispatcher.RemoveHandler(_backButtonHandler);
        base.Deactivate();
    }

    public override void Dispose()
    {
        NavigatorBackButtonDispatcher.RemoveHandler(_backButtonHandler);

        foreach (var route in _history.ToArray())
        {
            route.Dispose();
            route.Detach();
        }

        _history.Clear();
        foreach (var observer in _observers)
        {
            if (ReferenceEquals(observer.Navigator, this))
            {
                observer.Navigator = null;
            }
        }

        _observers.Clear();
        base.Dispose();
    }

    public override Widget Build(BuildContext context)
    {
        var route = CurrentRoute;
        Widget child = route == null
            ? new SizedBox()
            : new ActiveRouteHost(route);
        return new NavigatorScope(this, child);
    }

    public void Push(Route route)
    {
        if (route == null)
        {
            throw new ArgumentNullException(nameof(route));
        }

        SetState(() =>
        {
            var previousRoute = CurrentRoute;
            InstallRoute(route);
            previousRoute?.DidChangeNext(route);
            route.DidChangePrevious(previousRoute);
            route.DidPush();
            NotifyObserversPush(route, previousRoute);
        });
    }

    public bool MaybePop(object? result = null)
    {
        if (!CanPop)
        {
            return false;
        }

        var route = CurrentRoute;
        if (route == null || !route.WillPop())
        {
            return false;
        }

        SetState(() => PopCurrentRoute(result));
        return true;
    }

    public void Pop(object? result = null)
    {
        if (!MaybePop(result))
        {
            throw new InvalidOperationException("Navigator cannot pop the current route.");
        }
    }

    public void PushReplacement(Route newRoute, object? result = null)
    {
        if (newRoute == null)
        {
            throw new ArgumentNullException(nameof(newRoute));
        }

        SetState(() =>
        {
            var oldRoute = CurrentRoute;
            if (oldRoute == null)
            {
                InstallRoute(newRoute);
                newRoute.DidPush();
                NotifyObserversPush(newRoute, previousRoute: null);
                _ = result;
                return;
            }

            if (ReferenceEquals(oldRoute, newRoute))
            {
                throw new InvalidOperationException("Cannot replace a route with itself.");
            }

            var previousRoute = _history.Count > 1 ? _history[^2] : null;
            _history[^1] = newRoute;
            InstallRoute(newRoute);
            previousRoute?.DidChangeNext(newRoute);
            newRoute.DidChangePrevious(previousRoute);
            newRoute.DidPush();

            oldRoute.DidPop(previousRoute);
            oldRoute.Dispose();
            oldRoute.Detach();
            previousRoute?.DidPopNext(oldRoute);
            NotifyObserversReplace(newRoute, oldRoute);
            _ = result;
        });
    }

    private void PushInitialRoute()
    {
        if (_history.Count > 0)
        {
            return;
        }

        var initialRoute = CurrentWidget.InitialRoute;
        InstallRoute(initialRoute);
        initialRoute.DidPush();
        NotifyObserversPush(initialRoute, previousRoute: null);
    }

    private void PopCurrentRoute(object? result)
    {
        if (_history.Count == 0)
        {
            return;
        }

        var route = _history[^1];
        _history.RemoveAt(_history.Count - 1);
        var previousRoute = CurrentRoute;

        route.DidPop(previousRoute);
        route.Dispose();
        route.Detach();

        previousRoute?.DidPopNext(route);
        previousRoute?.DidChangeNext(nextRoute: null);

        _ = result;
        NotifyObserversPop(route, previousRoute);
    }

    private void InstallRoute(Route route)
    {
        if (route.Navigator != null && !ReferenceEquals(route.Navigator, this))
        {
            throw new InvalidOperationException("Route is already attached to another Navigator.");
        }

        if (!ReferenceEquals(route.Navigator, this))
        {
            route.Attach(this);
        }

        if (_history.Count == 0 || !ReferenceEquals(_history[^1], route))
        {
            _history.Add(route);
        }
    }

    private void SyncObservers(
        IReadOnlyList<NavigatorObserver> oldObservers,
        IReadOnlyList<NavigatorObserver> newObservers)
    {
        foreach (var oldObserver in oldObservers)
        {
            if (newObservers.Contains(oldObserver))
            {
                continue;
            }

            _observers.Remove(oldObserver);
            if (ReferenceEquals(oldObserver.Navigator, this))
            {
                oldObserver.Navigator = null;
            }
        }

        foreach (var observer in newObservers)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }

            observer.Navigator = this;
        }
    }

    private void NotifyObserversPush(Route route, Route? previousRoute)
    {
        foreach (var observer in _observers.ToArray())
        {
            observer.DidPush(route, previousRoute);
        }
    }

    private void NotifyObserversPop(Route route, Route? previousRoute)
    {
        foreach (var observer in _observers.ToArray())
        {
            observer.DidPop(route, previousRoute);
        }
    }

    private void NotifyObserversReplace(Route newRoute, Route? oldRoute)
    {
        foreach (var observer in _observers.ToArray())
        {
            observer.DidReplace(newRoute, oldRoute);
        }
    }

    private bool HandleBackButton()
    {
        if (!Element.IsActive)
        {
            return false;
        }

        return MaybePop();
    }
}

internal sealed class NavigatorScope : InheritedWidget
{
    public NavigatorScope(
        NavigatorState navigator,
        Widget child,
        Key? key = null) : base(key)
    {
        Navigator = navigator;
        Child = child;
    }

    public NavigatorState Navigator { get; }

    public Widget Child { get; }

    public override Widget Build(BuildContext context)
    {
        return Child;
    }

    protected internal override bool UpdateShouldNotify(InheritedWidget oldWidget)
    {
        return !ReferenceEquals(((NavigatorScope)oldWidget).Navigator, Navigator);
    }
}

internal sealed class ActiveRouteHost : StatelessWidget
{
    private readonly Route _route;

    public ActiveRouteHost(Route route)
    {
        _route = route;
    }

    public override Widget Build(BuildContext context)
    {
        return _route.BuildPage(context);
    }
}

internal static class NavigatorBackButtonDispatcher
{
    private static readonly List<Func<bool>> Handlers = [];

    public static void AddHandler(Func<bool> handler)
    {
        RemoveHandler(handler);
        Handlers.Add(handler);
    }

    public static void RemoveHandler(Func<bool> handler)
    {
        Handlers.RemoveAll(existing => ReferenceEquals(existing, handler));
    }

    public static bool DispatchBackButton()
    {
        for (var index = Handlers.Count - 1; index >= 0; index -= 1)
        {
            if (Handlers[index]())
            {
                return true;
            }
        }

        return false;
    }
}
