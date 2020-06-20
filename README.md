# GBAC-
After the wild success of the NES emulator I'm going to attempt to make a GBA emulator

Done:
  - ARM instructions
  - THUMB instructions
  - Render mode 0, 1, 2, 3, 4 (, 5?)
  - SWI's
  - Most IRQ handling (ohhh boy was this annoying)
  - DMA
  - Timers (though still pretty inacurrate)
  - Windowing
  - Alpha Blending
  - sound
  
To do:
  - SIO
  - better open bus
  - more accurate cycles
  - windowing in BM rendering modes
  - RTC

Known bugs:
  - Pokemon pinball menu screen popup should be transparent (alphablending issue, used to work)
  - Zelda LTTP does not boot past game selection
  - WarioWare and Doom audio is messed up (FIFO keeps reading when it shouldn't...)
  
