import 'package:flutter/material.dart';

import 'align_demo_page.dart';
import 'aspect_ratio_demo_page.dart';
import 'counter_screen.dart';
import 'counter_widgets.dart';
import 'container_demo_page.dart';
import 'custom_slivers_demo_page.dart';
import 'decorated_box_demo_page.dart';
import 'editable_text_demo_page.dart';
import 'fitted_box_demo_page.dart';
import 'fractionally_sized_box_demo_page.dart';
import 'grid_view_demo_page.dart';
import 'list_view_fixed_extent_demo_page.dart';
import 'list_view_reverse_demo_page.dart';
import 'list_view_separated_demo_page.dart';
import 'navigator_demo_page.dart';
import 'proxy_widgets_demo_page.dart';
import 'sample_routes.dart';
import 'scrollbar_demo_page.dart';
import 'stack_demo_page.dart';

class SampleGalleryScreen extends StatelessWidget {
  const SampleGalleryScreen({super.key});

  static final List<SampleRouteDefinition> _demoPages = <SampleRouteDefinition>[
    SampleRouteDefinition(
      routeName: SampleRoutes.counter,
      title: 'Counter',
      subtitle: 'existing sample',
      builder: () => const CounterScreen(),
    ),
    SampleRouteDefinition(
      routeName: SampleRoutes.navigator,
      title: 'Navigator',
      subtitle: 'named routes + RouteData + stack APIs',
      builder: () => const NavigatorDemoPage(),
    ),
    SampleRouteDefinition(
      routeName: SampleRoutes.listViewSeparated,
      title: 'ListView.Separated',
      subtitle: 'item + separator builder',
      builder: () => const ListViewSeparatedDemoPage(),
    ),
    SampleRouteDefinition(
      routeName: SampleRoutes.listViewFixedExtent,
      title: 'ListView fixed extent',
      subtitle: 'itemExtent + padding',
      builder: () => const ListViewFixedExtentDemoPage(),
    ),
    SampleRouteDefinition(
      routeName: SampleRoutes.listViewReverse,
      title: 'ListView reverse',
      subtitle: 'reverse=true behavior',
      builder: () => const ListViewReverseDemoPage(),
    ),
    SampleRouteDefinition(
      routeName: SampleRoutes.gridView,
      title: 'GridView + SliverGrid',
      subtitle: 'delegate-based 2D layout',
      builder: () => const GridViewDemoPage(),
    ),
    SampleRouteDefinition(
      routeName: SampleRoutes.customSlivers,
      title: 'Custom slivers',
      subtitle: 'SliverPadding + SliverFixedExtentList',
      builder: () => const CustomSliversDemoPage(),
    ),
    SampleRouteDefinition(
      routeName: SampleRoutes.scrollbar,
      title: 'Scrollbar',
      subtitle: 'controller + thumb',
      builder: () => const ScrollbarDemoPage(),
    ),
    SampleRouteDefinition(
      routeName: SampleRoutes.editableText,
      title: 'EditableText',
      subtitle: 'focus + IME + multiline caret',
      builder: () => const EditableTextDemoPage(),
    ),
    SampleRouteDefinition(
      routeName: SampleRoutes.proxyWidgets,
      title: 'Proxy widgets',
      subtitle: 'Opacity + Transform + ClipRect composition',
      builder: () => const ProxyWidgetsDemoPage(),
    ),
    SampleRouteDefinition(
      routeName: SampleRoutes.align,
      title: 'Align + Center',
      subtitle: 'single-child alignment and shrink factors',
      builder: () => const AlignDemoPage(),
    ),
    SampleRouteDefinition(
      routeName: SampleRoutes.stack,
      title: 'Stack + Positioned',
      subtitle: 'multi-child overlay layout',
      builder: () => const StackDemoPage(),
    ),
    SampleRouteDefinition(
      routeName: SampleRoutes.decoratedBox,
      title: 'DecoratedBox',
      subtitle: 'border + radius + fill decoration',
      builder: () => const DecoratedBoxDemoPage(),
    ),
    SampleRouteDefinition(
      routeName: SampleRoutes.container,
      title: 'Container',
      subtitle: 'alignment + margin + constraints + transform',
      builder: () => const ContainerDemoPage(),
    ),
    SampleRouteDefinition(
      routeName: SampleRoutes.aspectRatio,
      title: 'AspectRatio + Spacer',
      subtitle: 'tight ratio layout + flex gap',
      builder: () => const AspectRatioDemoPage(),
    ),
    SampleRouteDefinition(
      routeName: SampleRoutes.fractionallySizedBox,
      title: 'FractionallySizedBox',
      subtitle: 'fractional constraints + alignment',
      builder: () => const FractionallySizedBoxDemoPage(),
    ),
    SampleRouteDefinition(
      routeName: SampleRoutes.fittedBox,
      title: 'FittedBox',
      subtitle: 'box-fit scaling + alignment',
      builder: () => const FittedBoxDemoPage(),
    ),
  ];

  static final Map<String, SampleRouteDefinition> _demoPageByRoute =
      <String, SampleRouteDefinition>{
        for (final SampleRouteDefinition page in _demoPages)
          page.routeName: page,
      };

  @override
  Widget build(BuildContext context) {
    return Navigator(
      onGenerateRoute: _buildRoute,
      observers: <NavigatorObserver>[SampleNavigationObservers.pageRoutes],
      initialRoute: SampleRoutes.menu,
    );
  }

  static Route<dynamic> _buildRoute(RouteSettings settings) {
    final RouteData routeData = _routeDataFromSettings(settings);

    if (routeData.name == SampleRoutes.menu) {
      return MaterialPageRoute<void>(
        builder: (BuildContext context) => SampleMenuPage(pages: _demoPages),
        settings: settings,
      );
    }

    if (routeData.name == SampleRoutes.navigatorDetails) {
      return MaterialPageRoute<void>(
        builder: (BuildContext context) => SampleDemoPage(
          title: 'Navigator details',
          subtitle: 'RouteData query/arguments + push/pop operations',
          child: NavigatorDetailsPage(routeData: routeData),
        ),
        settings: settings,
      );
    }

    final SampleRouteDefinition? page = _demoPageByRoute[routeData.name];
    if (page != null) {
      return MaterialPageRoute<void>(
        builder: (BuildContext context) =>
            SampleDemoPage.fromDefinition(page: page, child: page.builder()),
        settings: settings,
      );
    }

    return MaterialPageRoute<void>(
      builder: (BuildContext context) =>
          SampleUnknownRoutePage(routeName: settings.name ?? '(null)'),
      settings: settings,
    );
  }

  static RouteData _routeDataFromSettings(RouteSettings settings) {
    if (settings.arguments is RouteData) {
      final RouteData routeData = settings.arguments! as RouteData;
      return RouteData.fromLocation(
        settings.name ?? routeData.location,
        arguments: routeData.arguments,
      );
    }

    return RouteData.fromLocation(
      settings.name ?? SampleRoutes.menu,
      arguments: settings.arguments,
    );
  }
}

class SampleMenuPage extends StatelessWidget {
  const SampleMenuPage({required this.pages, super.key});

  final List<SampleRouteDefinition> pages;

  @override
  Widget build(BuildContext context) {
    return Container(
      color: Colors.white,
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        spacing: 10,
        children: <Widget>[
          const Text(
            'Flutter.Net widget pages',
            style: TextStyle(fontSize: 24, color: Colors.black),
          ),
          const Text(
            'Route-based sample menu. Open page and return via Back button or Esc.',
            style: TextStyle(fontSize: 14, color: Colors.black54),
          ),
          Expanded(
            child: ListView.builder(
              itemCount: pages.length,
              itemExtent: 56,
              padding: const EdgeInsets.fromLTRB(0, 8, 0, 8),
              itemBuilder: (BuildContext itemContext, int index) {
                return _buildPageButton(context, pages[index]);
              },
            ),
          ),
        ],
      ),
    );
  }

  static Widget _buildPageButton(
    BuildContext context,
    SampleRouteDefinition page,
  ) {
    return CounterTapButton(
      label: '${page.title}  |  ${page.subtitle}',
      onTap: () => Navigator.of(context).pushNamed(page.routeName),
      background: const Color(0xFFDCE3ED),
      foreground: Colors.black,
      fontSize: 12,
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
    );
  }
}

class SampleDemoPage extends StatelessWidget {
  const SampleDemoPage({
    required this.title,
    required this.subtitle,
    required this.child,
    super.key,
  });

  SampleDemoPage.fromDefinition({
    required SampleRouteDefinition page,
    required this.child,
    super.key,
  }) : title = page.title,
       subtitle = page.subtitle;

  final String title;
  final String subtitle;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Container(
      color: Colors.white,
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        spacing: 10,
        children: <Widget>[
          Row(
            spacing: 8,
            children: <Widget>[
              SizedBox(
                width: 90,
                child: CounterTapButton(
                  label: 'Back',
                  onTap: () => Navigator.of(context).maybePop(),
                  background: Colors.blue,
                  foreground: Colors.white,
                  fontSize: 12,
                  padding: const EdgeInsets.symmetric(
                    horizontal: 10,
                    vertical: 8,
                  ),
                ),
              ),
              Expanded(
                child: Text(
                  title,
                  style: const TextStyle(fontSize: 22, color: Colors.black),
                ),
              ),
            ],
          ),
          Text(
            subtitle,
            style: const TextStyle(fontSize: 14, color: Colors.black54),
          ),
          Expanded(
            child: Container(
              color: const Color(0xFFF7F9FC),
              padding: const EdgeInsets.all(12),
              child: child,
            ),
          ),
        ],
      ),
    );
  }
}

class SampleUnknownRoutePage extends StatelessWidget {
  const SampleUnknownRoutePage({required this.routeName, super.key});

  final String routeName;

  @override
  Widget build(BuildContext context) {
    return Container(
      color: Colors.white,
      alignment: Alignment.center,
      child: Text(
        'Unknown route: $routeName',
        style: const TextStyle(fontSize: 16, color: Colors.black),
      ),
    );
  }
}
