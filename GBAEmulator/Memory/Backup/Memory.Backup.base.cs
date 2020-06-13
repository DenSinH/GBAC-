using System;
using System.Collections.Generic;
using System.Text;

namespace GBAEmulator.Memory.Backup
{
    interface IBackup
    {
        /// <summary>
        /// Initialize storage (usually to 0xFF)
        /// </summary>
        public void Init();

        /// <summary>
        /// Dump memory to file
        /// </summary>
        /// <param name="FileName"></param>
        public void Dump(string FileName);

        /// <summary>
        /// Load storage from file
        /// </summary>
        /// <param name="FileName">Save file location</param>
        public void Load(string FileName);

        /// <summary>
        /// Read byte from address
        /// 
        /// EEPROM actually does not need an address, so we overload it
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte Read(uint address);

        /// <summary>
        /// Write value to address
        /// 
        /// EEPROM actually does not need an address, so we overload it
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns>Returns wether the memory has actually changed</returns>
        public bool Write(uint address, byte value);
    }
}
