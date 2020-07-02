import re

GBATekEntry = """
GBA  NDS7 NDS9 DSi7 DSi9 Basic Functions
  00h  00h  00h  -    -    SoftReset
  01h  -    -    -    -    RegisterRamReset
  02h  06h  06h  06h  06h  Halt
  03h  07h  -    07h  -    Stop/Sleep
  04h  04h  04h  04h  04h  IntrWait       ;DSi7/DSi9: both bugged?
  05h  05h  05h  05h  05h  VBlankIntrWait ;DSi7/DSi9: both bugged?
  06h  09h  09h  09h  09h  Div
  07h  -    -    -    -    DivArm
  08h  0Dh  0Dh  0Dh  0Dh  Sqrt
  09h  -    -    -    -    ArcTan
  0Ah  -    -    -    -    ArcTan2
  0Bh  0Bh  0Bh  0Bh  0Bh  CpuSet
  0Ch  0Ch  0Ch  0Ch  0Ch  CpuFastSet
  0Dh  -    -    -    -    GetBiosChecksum
  0Eh  -    -    -    -    BgAffineSet
  0Fh  -    -    -    -    ObjAffineSet
  GBA  NDS7 NDS9 DSi7 DSi9 Decompression Functions
  10h  10h  10h  10h  10h  BitUnPack
  11h  11h  11h  11h  11h  LZ77UnCompReadNormalWrite8bit   ;"Wram"
  12h  -    -    -    -    LZ77UnCompReadNormalWrite16bit  ;"Vram"
  -    -    -    01h  01h  LZ77UnCompReadByCallbackWrite8bit
  -    12h  12h  02h  02h  LZ77UnCompReadByCallbackWrite16bit
  -    -    -    19h  19h  LZ77UnCompReadByCallbackWrite16bit (same as above)
  13h  -    -    -    -    HuffUnCompReadNormal
  -    13h  13h  13h  13h  HuffUnCompReadByCallback
  14h  14h  14h  14h  14h  RLUnCompReadNormalWrite8bit     ;"Wram"
  15h  -    -    -    -    RLUnCompReadNormalWrite16bit    ;"Vram"
  -    15h  15h  15h  15h  RLUnCompReadByCallbackWrite16bit
  16h  -    16h  -    16h  Diff8bitUnFilterWrite8bit       ;"Wram"
  17h  -    -    -    -    Diff8bitUnFilterWrite16bit      ;"Vram"
  18h  -    18h  -    18h  Diff16bitUnFilter
  GBA  NDS7 NDS9 DSi7 DSi9 Sound (and Multiboot/HardReset/CustomHalt)
  19h  08h  -    08h  -    SoundBias
  1Ah  -    -    -    -    SoundDriverInit
  1Bh  -    -    -    -    SoundDriverMode
  1Ch  -    -    -    -    SoundDriverMain
  1Dh  -    -    -    -    SoundDriverVSync
  1Eh  -    -    -    -    SoundChannelClear
  1Fh  -    -    -    -    MidiKey2Freq
  20h  -    -    -    -    SoundWhatever0
  21h  -    -    -    -    SoundWhatever1
  22h  -    -    -    -    SoundWhatever2
  23h  -    -    -    -    SoundWhatever3
  24h  -    -    -    -    SoundWhatever4
  25h  -    -    -    -    MultiBoot
  26h  -    -    -    -    HardReset
  27h  1Fh  -    1Fh  -    CustomHalt
  28h  -    -    -    -    SoundDriverVSyncOff
  29h  -    -    -    -    SoundDriverVSyncOn
  2Ah  -    -    -    -    SoundGetJumpList
  GBA  NDS7 NDS9 DSi7 DSi9 New NDS Functions
  -    03h  03h  03h  03h  WaitByLoop
  -    0Eh  0Eh  0Eh  0Eh  GetCRC16
  -    0Fh  0Fh  -    -    IsDebugger
  -    1Ah  -    1Ah  -    GetSineTable
  -    1Bh  -    1Bh  -    GetPitchTable (DSi7: bugged)
  -    1Ch  -    1Ch  -    GetVolumeTable
  -    1Dh  -    1Dh  -    GetBootProcs (DSi7: only 1 proc)
  -    -    1Fh  -    1Fh  CustomPost
  GBA  NDS7 NDS9 DSi7 DSi9 New DSi Functions (RSA/SHA1)
  -    -    -    20h  20h  RSA_Init_crypto_heap
  -    -    -    21h  21h  RSA_Decrypt
  -    -    -    22h  22h  RSA_Decrypt_Unpad
  -    -    -    23h  23h  RSA_Decrypt_Unpad_OpenPGP_SHA1
  -    -    -    24h  24h  SHA1_Init
  -    -    -    25h  25h  SHA1_Update
  -    -    -    26h  26h  SHA1_Finish
  -    -    -    27h  27h  SHA1_Init_update_fin
  -    -    -    28h  28h  SHA1_Compare_20_bytes
  -    -    -    29h  29h  SHA1_Random_maybe
  GBA  NDS7 NDS9 DSi7 DSi9 Invalid Functions
  2Bh+ 20h+ 20h+ -    -    Crash (SWI xxh..FFh do jump to garbage addresses)
  -    xxh  xxh  -    -    Jump to 0   (on any SWI numbers not listed above)
  -    -    -    12h  12h  No function (ignored)
  -    -    -    2Bh  2Bh  No function (ignored)
  -    -    -    40h+ 40h+ Mirror      (SWI 40h..FFh mirror to 00h..3Fh)
  -    -    -    xxh  xxh  Hang        (on any SWI numbers not listed above)
"""

# my own dump of the BIOS of course
with open("./gba_bios.bin", "rb") as f:
    bios = bytearray(f.read())


def get_word_at(address: int):
    return bios[address] | (bios[address + 1] << 8) | (bios[address + 2] << 16) | (bios[address + 3] << 24)


for line in GBATekEntry.split("\n"):
    split_line = re.split(r"\s+", line, 6)
    if len(split_line) < 6:
        continue

    try:
        code = int(split_line[1].strip("h"), 16)
        method = split_line[-1]
        print(f"{hex(code)} (@{hex(0x1c8 + (code << 2))} = {hex(get_word_at(0x1c8 + (code << 2)))})".ljust(30, " ") + method)
    except ValueError:
        pass
