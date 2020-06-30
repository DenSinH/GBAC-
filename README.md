# GBAC-
After the wild success of the NES emulator I'm going to attempt to make a GBA emulator

### Controls:
| XInput Controller | Keyboard |    | GBA|
|-------------------|----------|----|----|
| A | Z | -> | A | 
| B/X | X | -> | B | 
| LShoulder| LShift | -> | L |
| RShoulder| C | -> | R |
| Start | A | -> | Start |
| Back | B | -> | Select |
| DPad Up / Left Joystick| Up | -> | DPad Up |
| DPad Down / Left Joystick| Down | -> | DPad Down |
| DPad Left / Left Joystick| Left | -> | DPad Left |
| DPad Right / Left Joystick| Right | -> | DPad Right |

Hotkeys:
 - F4: Pause emulation
 
### Rendering modes:
I added 2 modes of rendering, threaded and not threaded. Threaded rendering will give a performance boost in general. Then for threaded rendering you can enable unsafe rendering, where VRAM/OAM/PAL changes are executed whether the PPU was rendering or not. These can be toggled in the .csproj file, by adding the compiler symbols `THREADED_RENDERING` and `UNSAFE_RENDERING`. For `UNSAFE_RENDERING` to work, `THREADED_RENDERING` has to be enabled as well. 

I have not noticed any differences between `UNSAFE_RENDERING` and not, except it _has_ crashed in unsafe mode (when it tried to read an out of bounds VRAM entry while rendering, which probably got changed in a different thread). I have prevented the crash as well as I could, but in case it happens, a pixel might be off. It could also happen that PAL/OAM writes happen, but I do not think they will be of much hinderance. There is some performance gain in turning on `UNSAFE_RENDERING`.

#### Done:
  - ARM instructions
  - THUMB instructions
  - Render mode 0, 1, 2, 3, 4 (, 5?)
  - SWI's
  - IRQ handling (ohhh boy was this annoying)
  - DMA
  - Timers (though not always acurrate)
  - Windowing
  - Alpha Blending
  - Sound
  - Debugging options (viewing some registers / charblocks, enabling rendering layers/GFX)
  - RTC
  
#### To do:
  - SIO
  - better open bus
  - more accurate cycles
  - windowing in BM rendering modes
  - More GPIO Ports
  - Controls mapping

#### Known bugs:
  - Pokemon pinball menu screen popup should be transparent (alphablending issue, used to work)
  - Mario & Luigi Superstar Saga video is messed up
  
