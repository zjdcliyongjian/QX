import Cocoa
import WebKit

final class AppDelegate: NSObject, NSApplicationDelegate, WKScriptMessageHandler, WKNavigationDelegate {
    private var window: NSWindow!
    private var webView: WKWebView!
    private var keyMonitor: Any?

    func applicationDidFinishLaunching(_ notification: Notification) {
        let controller = WKUserContentController()
        controller.add(self, name: "app")

        let configuration = WKWebViewConfiguration()
        configuration.userContentController = controller
        configuration.mediaTypesRequiringUserActionForPlayback = []

        webView = WKWebView(frame: .zero, configuration: configuration)
        webView.navigationDelegate = self
        webView.setValue(false, forKey: "drawsBackground")

        let screenFrame = NSScreen.main?.frame ?? NSRect(x: 0, y: 0, width: 1440, height: 900)
        window = NSWindow(
            contentRect: screenFrame,
            styleMask: [.borderless],
            backing: .buffered,
            defer: false
        )
        window.backgroundColor = NSColor(calibratedRed: 0.016, green: 0.027, blue: 0.086, alpha: 1)
        window.contentView = webView
        window.collectionBehavior = [.fullScreenPrimary, .canJoinAllSpaces]
        window.isReleasedWhenClosed = false
        window.makeKeyAndOrderFront(nil)
        window.makeFirstResponder(webView)

        NSApp.activate(ignoringOtherApps: true)
        NSApp.presentationOptions = [.autoHideDock, .autoHideMenuBar]

        keyMonitor = NSEvent.addLocalMonitorForEvents(matching: .keyDown) { event in
            if event.keyCode == 53 {
                NSApp.terminate(nil)
                return nil
            }
            return event
        }

        guard let htmlURL = Bundle.main.resourceURL?.appendingPathComponent("index.html") else {
            showErrorAndQuit("找不到动画页面 index.html")
            return
        }
        webView.loadFileURL(htmlURL, allowingReadAccessTo: htmlURL.deletingLastPathComponent())
    }

    func applicationWillTerminate(_ notification: Notification) {
        if let monitor = keyMonitor {
            NSEvent.removeMonitor(monitor)
        }
        webView?.configuration.userContentController.removeScriptMessageHandler(forName: "app")
    }

    func applicationShouldTerminateAfterLastWindowClosed(_ sender: NSApplication) -> Bool {
        return true
    }

    func userContentController(_ userContentController: WKUserContentController, didReceive message: WKScriptMessage) {
        if message.name == "app", String(describing: message.body) == "close" {
            NSApp.terminate(nil)
        }
    }

    func webView(_ webView: WKWebView, didFinish navigation: WKNavigation!) {
        let musicURL = Bundle.main.bundleURL
            .deletingLastPathComponent()
            .appendingPathComponent("传奇.mp3")

        guard FileManager.default.fileExists(atPath: musicURL.path),
              let jsonData = try? JSONEncoder().encode(musicURL.absoluteString),
              let jsonString = String(data: jsonData, encoding: .utf8) else {
            return
        }
        webView.evaluateJavaScript("window.setMusicUrl(\(jsonString))", completionHandler: nil)
    }

    private func showErrorAndQuit(_ message: String) {
        let alert = NSAlert()
        alert.messageText = "七夕浪漫3D爱心粒子"
        alert.informativeText = message
        alert.alertStyle = .critical
        alert.runModal()
        NSApp.terminate(nil)
    }
}

let application = NSApplication.shared
let delegate = AppDelegate()
application.delegate = delegate
application.setActivationPolicy(.regular)
application.run()
