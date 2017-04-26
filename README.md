# Stepquencer
By Gabriel Brown, Mani Diaz, Roland Munsil, and Paige Pfeiffer

## How to Install and Run on iOS

**Requirements**
- A Mac
- XCode
- Xamarin Studio

**Steps**
1) Start up Xamarin Studio and load in our files
2) Also start up Xcode and:
  - Choose Xcode Menu >Preferences... 
  - Click on Accounts tab
  - Click + button and select "Add Apple ID"
  - Log in with your Apple ID and password (or create one if you don't have one)
  - While still in the accounts tab, click on your account and select "View Details"
  - Click the "Create" button next to **_iOS Development_** (this will give you a Signing identity)
  - Review and accept what Apple throws at you
  - Plug in the iOS device you wish to deploy to 
  - Create a new **_Single-view iOS_** project in Xcode, and leave it blank
  - Under the *General > Identity section* of this new Xcode project, choose your bundle identifier 
  (something like com.YOURNAME.Stepquencer)
  - Right below where you specify the bundle identifier, check the **_Automatically manage signin_** box
  - In Xcode, ensure that your device is selected and install this blank app with the correct bundle identifier
3) Now go back to Xamarin Studio. Open up Info.plist under the Stepquencer.iOS project
4) Ensure that the bundle identifier in Info.plist is the SAME AS the one you just specified in Xcode
5) Right click on the Stepquencer.iOS project and click on "Options"
6) Go to the section labeled "iOS Bundle Signing" and explicitly set YOUR **_signing identity_** and **_provisioning profile_**
7) Go back to the solution, make sure that:
  - "Stepquencer.iOS" is selected
  - "Release | iPhone" is selected
  - Your device is selected
8) Now hit the play button, and it will attempt to install but fail because you have to give your account access.
To remedy this:
  - After having tried to install, go to **_Settings > General > Device Management_**
  - Click on your account name (should be your apple ID)
  - Trust your account
9) After telling your iOS device that it can indeed trust your account, hit the play button in Xamarin Studio one more 
time to truly install the app.


## How to Install and Run on Android

**Requirements**
- Xamarin Studio (Mac) or Visual Studio (Other)

**Steps**
1) Start up your IDE and load in our files
2) Connect your Android device via USB
3) Go to the top left corner of the IDE window (near the play button) and ensure that:
  - "Stepquencer.Droid" is selected
  - "Debug" or "Release" is selected
  - Your device is selected (not an emulator)
4) Simply hit play and Stepquencer will install on your device
  
