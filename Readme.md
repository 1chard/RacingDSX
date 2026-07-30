![GitHub all releases](https://img.shields.io/github/downloads/1chard/racingDSX/total)

Works with DSX, but since version 0.8.0, it is optional.
Report bugs with built-in controller implementation here: [issues](https://github.com/1chard/RacingDualSense/issues)

Add Dualsense's dynamic trigger support for Forza and Dirt
## Supported games:  
- [Forza Horizon 6](https://github.com/1chard/RacingDualSense/wiki/Forza-Horizon-6) 
- [Forza Horizon 5](https://github.com/1chard/RacingDualSense/wiki/Forza-Horizon-5) 
- [Forza Horizon 4](https://github.com/1chard/RacingDualSense/wiki/Forza-Horizon-4)
- Forza Motorsport 7/8
- Dirt Rally 1/2

## How to install (example for Forza Horizon 6 on Steam)
1. Install latest version of release: https://github.com/1chard/RacingDualSense/releases/latest
2. Run RacingDualSense.exe
3. Allow firewall if necessary
4. Launch Forza Horizon 6
5. In the menu, open Settings > HUD and Gameplay
6. Set DATA OUT to ON, set DATA OUT IP ADDRESS to 127.0.0.1 and set DATA OUT IP PORT to 5300
7. (Optional) If you want to use DSX or [DualSenseY](https://github.com/WujekFoliarz/DualSenseY-v2) in the background, put UDP port to 6969 on "DSX Port" input

## Note for Microsoft Store
Telemetry data may not be received on Xbox/Microsoft Store games, you need to enable UDP Loopback
1. Install [Window 8 AppContainer Loopback Utility](https://telerik-fiddler.s3.amazonaws.com/fiddler/addons/enableloopbackutility.exe)
2. Start the utility (if it shows a message about orphan sid, you can safely ignore it)
3. Make sure that Forza Horizon 4 / Motorsport 7 are checked
4. Save changes

## Launch options
- `--nogui`, `--headless` Launches the application without the GUI, loads active configuration file
- `--attach` Try 10 seconds to attach to game, if successful starts to track game's lifespan, when the game is closed, RacingDualSense will automatically close as well
- `--run` Runs and attaches to game process, when the game is closed, RacingDualSense will automatically close

If you want to auto start RacingDualSense with a Steam game without the interface, follow the steps:  
1. Open game properties
<img width="258" height="200" alt="image" src="https://github.com/user-attachments/assets/180f4e5d-5fb9-40a7-9a5a-1cb17626ffde" />
 
2. Put the Launch Options according to your case:
- If using DSX: run the game from RacingDualSense: `"C:\Path\To\RacingDualSense.exe" --headless --run %command%`  
- If using new built-in controller: rename executable from `RacingDualSense.exe` to `RacingDualSenseHeadless.exe`, then put: `"C:\Windows\System32\cmd.exe" /c "start "" explorer.exe "C:\Path\To\RacingDualSenseHeadless.exe" && start "" %command%"`

Use tray's "Open Interface" option to open GUI if you started RacingDualSense with `--nogui` or `--headless`.

-----------------------------------------------------------------------------------------------------------------------------------------

## Setting up DiRT Rally 1 / 2 for UDP Connection:
1. Go to `C:\Users\<USER>\Documents\My Games\DiRT Rally X.0\hardwaresettings`;
2. Open `hardware_settings_config` file with your favorite text editor;
3. Find for **udp** tag and configure as shown below:
      ```xml
      <motion_platform>
           ...
           <udp enabled="true" extradata="3" ip="127.0.0.1" port="5300" delay="1" />
           ...
      </motion_platform>
      ```
   - **enabled = true**
   - **extradata = 3**
   - **port = 5300**

-----------------------------------------------------------------------------------------------------------------------------------------

## Note for Forza Motorsport 7
1. Launch the game and head to the HUD options menu
2. Set Data Out to ON
3. Set Data Out IP Address to 127.0.0.1 (localhost)
4. Set Data Out IP Port to 5300
5. Set Data Out Packet Format to CAR DASH

-----------------------------------------------------------------------------------------------------------------------------------------
🔺🔺 It is REQUIRED to install .NET8 for racingDSX to work at all!🔺🔺           
Download .NET8.0 from the link here: https://dotnet.microsoft.com/en-us/download

## Thanks and Credits

[DualSenseX](https://github.com/Paliverse/DualSenseX)

[DualSenseY](https://github.com/WujekFoliarz/DualSenseY-v2)

[Forza-Telemetry](https://github.com/austinbaccus/forza-telemetry/tree/main/ForzaCore)

[patmagauran](https://github.com/patmagauran)
