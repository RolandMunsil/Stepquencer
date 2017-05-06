# Stepquencer
By Gabriel Brown, Mani Diaz, Roland Munsil, and Paige Pfeiffer

## How to Install and Run on iOS

**Requirements**
- A Mac
- Xcode
- Xamarin Studio

**Steps**
1. Start up Xamarin Studio and load in our files
2. Also start up Xcode and:
   1. Choose Xcode Menu >Preferences... 
   2. Click on Accounts tab
   3. Click + button and select "Add Apple ID"
   4. Log in with your Apple ID and password (or create one if you don't have one)
   5. While still in the accounts tab, click on your account and select "View Details"
   6. Click the "Create" button next to **_iOS Development_** (this will give you a Signing identity)
   7. Review and accept what Apple throws at you
   8. Plug in the iOS device you wish to deploy to 
   9. Create a new **_Single-view iOS_** project in Xcode, and leave it blank
   10. Under the *General* section of your project window, look for the *Identity* heading and choose your bundle identifier 
   (something like com.YOURNAME.Stepquencer)
   11. Right below where you specify the bundle identifier, check the **_Automatically manage signin_** box
3. In Xcode, next to the build button in the top left corner, ensure that your device is selected and hit the build button
4. Xcode will attempt to install but fail because you have to give your account access. To remedy this:
   * After having tried to install, go to **_Settings > General > Device Management_**
   * Click on your account name (should be your apple ID)
   * Trust your account
5. After telling your iOS device that it can indeed trust your account, hit the play button in Xamarin Studio one more 
time to truly install the app.
6. Now go back to Xamarin Studio. Open up Info.plist under the Stepquencer.iOS project
7. Ensure that the bundle identifier in Info.plist is the SAME AS the one you just specified in Xcode
8. Right click on the "Stepquencer.iOS" project in the sidebar (NOT the plain "Stepquencer" project)
9. Go to the section labeled "iOS Bundle Signing" and explicitly set YOUR **_signing identity_** and **_provisioning profile_**
10. Go back to the solution, make sure that:
    * "Stepquencer.iOS" is selected
    * "Release | iPhone" is selected
    * Your device is selected
11. Now hit the play button, and it will attempt to install but fail because you have to give your account access.



## How to Install and Run on Android
1. Install Xamarin Studio (Mac) or Visual Studio with Xamarin (Windows): https://www.xamarin.com/download.
2. Once you've opened up our solution, running on the emulator should be easy - the emulator comes with Xamarin. Make sure Stepquencer.Droid > Debug > Android_Accelerated_x86 is selected on Mac, and on Windows it should be Debug > Any CPU > Android_Accelerated_x86. You may also need to set Stepquencer.Droid to be the startup project.
3. To run on an android phone, you first need to enable debugging on your phone. Instructions can be found here: https://www.kingoapp.com/root-tutorials/how-to-enable-usb-debugging-mode-on-android.htm
4. Now connect your phone to your computer with a USB cable. Your phone will ask if you want to allow USB debugging - hit OK.
5. In your IDE, you should be able to select your phone to debug on - simply hit run and it will build, deploy, and start debugging on your phone (If it doesn't show up, try restarting your IDE). Note that, at least on our phone, if you stop debugging the app will be uninstalled, so you need to disconnect your phone while debugging to get the app to stay. Also be aware that deploying can take a while.

## Code structure
The Stepquencer solution has 3 projects - Stepquencer, Stepquencer.Droid, and Stepquencer.iOS. Stepquencer contains code compiled for both platfoms, and the .Droid and .iOS contain platform-specific code. In our case, almost all of our code is in Stepquencer. The only things in the platform specific projects are app icons, splash screen stuff, and button images. However, note that we *do* have platform-specific code, but we have in the main Stepquencer project. To separate the different platforms we use Xamarin's \_\_ANDROID\_\_ and \_\_IOS\_\_ defines to compile platform-specific sections of code within a single file.

The layout of the Stepquencer project is as follows:

### Pages folder
Contains all of the UI pages (e.g. more options page, change instruments page). Each page has a .xaml file which describes some properties of the page and a .xaml.cs file which contains code for that page. The .xaml file can be used to specify the layout of a page, but we mostly specified page layout in the code by adding elements to the page in the page's constructor.

MainPage is the page that opens on startup. Because it's the first page, its constructor is also where we do all of our startup things (like create the grid, load instrument information, instantiate variables, etc.).

### UIElements folder
This has all of our custom UI elements.

### Audio
This constains all of the classes used to play audio. SongPlayer is the class that actually manages audio playback, and Instrument and Song are used by SongPlayer and MainPage to represent the song.

### Instruments
There is a WAV file for each instrument that is used to make all of the notes of an instrument. These are all embedded in the built app. The \_colors.txt file specifies the color used to represent an instrument - it is just a list in the format \<instrument name\>|\<hex code for color\>

The remaining files are:
   * App.xaml/App.xaml.cs - the class representing the entire program. Pretty much all it does is create the MainPage and stop the song when the user leaves the app.
   * CustomScrollViewRenderer.cs - used for scrollbar rendering
   * FileUtilities.cs - contains methods and variables to help load and save songs and embedded resources.
   * firsttimesong.txt - the song that the user sees the first time they open the app.

## Bugs and stuff
We tracked our bugs and TODOs on trello: https://trello.com/b/XU3SLcQ3/stepquencer
