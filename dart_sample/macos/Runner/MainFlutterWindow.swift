import Cocoa
import FlutterMacOS

class MainFlutterWindow: NSWindow {
  override func awakeFromNib() {
    let flutterViewController = FlutterViewController()
    let targetContentPixels = NSSize(width: 512, height: 1820)
    let backingScale = max(1.0, self.screen?.backingScaleFactor ?? NSScreen.main?.backingScaleFactor ?? 1.0)
    let targetContentSize = NSSize(
      width: targetContentPixels.width / backingScale,
      height: targetContentPixels.height / backingScale
    )
    let windowFrame = self.frame
    let targetFrameSize = self.frameRect(forContentRect: NSRect(origin: .zero, size: targetContentSize)).size
    self.contentViewController = flutterViewController
    self.setFrame(
      NSRect(
        x: windowFrame.origin.x,
        y: windowFrame.origin.y,
        width: targetFrameSize.width,
        height: targetFrameSize.height
      ),
      display: true
    )

    RegisterGeneratedPlugins(registry: flutterViewController)

    super.awakeFromNib()
  }
}
