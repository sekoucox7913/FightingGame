MobileNative - A unified interface to native features on mobile platforms for Unity.

Version 1.0.2

MobileNative is a Unity package that packs some useful native features on mobile platforms(currently iOS and Android), and provides same interfaces to make use of them.

Currently included features:
* Application Info
* Alert/Prompt
* Share images and messages with other applications
* Open App Store/Google Play page of applications
* Check and Prompt for upgrading applications
* Loading animation




######################################################################
>>> Application Info <<<
######################################################################

- appBundleID (readonly)
- appName     (readonly)
- appVersion  (readonly)
- appBuild    (readonly)

Get application bundle id, name, version and build(version code) through native interfaces.







######################################################################
>>> Alert/Prompt <<<
######################################################################

- Alert()

Alerts/Prompts a message, with a title and up to 3 buttons. Each button will trigger a callback if provided.
This feature is used in other features, such as "Prompt for upgrading".







######################################################################
>>> Share images and messages with other applications <<<
*** Requires iOS 6.0 or above. ***
######################################################################

- ShareMessage()

Shares a text message.


- ShareImage()

Shares an image with an optional message. But due to a system limitation on Android, the message may not be recognized by all applications.


- ShareScreenshot()

Shares a screenshot with an optional message. A shortcut to ShareImage() with a screenshot.







######################################################################
>>> Open App Store/Google Play page of applications <<<
*** Requires iOS 6.0 or above. ***
######################################################################

- ShowApp()

Shows an App Store page of an application in a popup on iOS, or application details page on Android.







######################################################################
>>> Check and Prompt for upgrading applications <<<
######################################################################

- UpgradeTest()

Optionally test for a new version of the application and prompts for upgrading.
Returns true if new version available and prompted for upgrading, false otherwise.







######################################################################
>>> Show a loading animation that covers the whole screen <<<
######################################################################

- ShowLoading()
- HideLoading()

Shows a loading animation that covers the whole screen, block further taps until hidden.







================================================================================

Feedback & Request

If you found issues or need new native features, please contact us.

miragine@gmail.com
