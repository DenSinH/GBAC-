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
  - Zelda LTTP does not boot past game selection
  
