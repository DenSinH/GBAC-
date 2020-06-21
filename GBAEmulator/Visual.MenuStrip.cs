using System;
using System.Windows.Forms;

namespace GBAEmulator
{
    partial class Visual
    {
        private void InitMenustrip(MenuStrip ms)
        {
            /* ========================================= Game Menu  ============================================== */
            ToolStripMenuItem GameMenu = new ToolStripMenuItem("Game");
            ToolStripMenuItem GameOpenItem = new ToolStripMenuItem("Open", null, new EventHandler(LoadGame));
            ToolStripMenuItem GameDebugItem = new ToolStripMenuItem("Debug", null, new EventHandler(OpenDebug));

            GameMenu.DropDownItems.Add(GameOpenItem);
            GameMenu.DropDownItems.Add(GameDebugItem);
            ((ToolStripDropDownMenu)(GameMenu.DropDown)).ShowImageMargin = false;
            ((ToolStripDropDownMenu)(GameMenu.DropDown)).ShowCheckMargin = false;

            /* ====================================== Emulation Menu ============================================ */
            ToolStripMenuItem EmulationMenu = new ToolStripMenuItem("Emulation");
            ToolStripMenuItem EmulationPauseItem = new ToolStripMenuItem("Pause", null,
                new EventHandler((object sender, EventArgs e) => {
                    this.gba.Pause ^= true;
                    ((ToolStripMenuItem)sender).Checked ^= true;
                }));
            ToolStripMenuItem EmulationMuteItem = new ToolStripMenuItem("Mute", null,
                new EventHandler((object sender, EventArgs e) => {
                    this.gba.apu.ExternalEnable = !(((ToolStripMenuItem)sender).Checked ^= true);
                }));

            EmulationMenu.DropDownItems.Add(EmulationPauseItem);
            EmulationMenu.DropDownItems.Add(EmulationMuteItem);

            /* ======================================== Video Menu ============================================== */
            ToolStripMenuItem VideoMenu = new ToolStripMenuItem("Video");
            for (int i = 0; i < 4; i++)
            {
                int BGno = i;
                ToolStripMenuItem BGxEnable = new ToolStripMenuItem($"BG{BGno} Enable", null,
                new EventHandler((object sender, EventArgs e) => {
                    this.gba.ppu.ExternalBGEnable[BGno] = (((ToolStripMenuItem)sender).Checked ^= true);
                }));
                BGxEnable.Checked = true;
                VideoMenu.DropDownItems.Add(BGxEnable);
            }

            ToolStripMenuItem OBJEnable = new ToolStripMenuItem("OBJ Enable", null,
                new EventHandler((object sender, EventArgs e) => {
                    this.gba.ppu.ExternalOBJEnable = (((ToolStripMenuItem)sender).Checked ^= true);
                }));
            OBJEnable.Checked = true;
            VideoMenu.DropDownItems.Add(OBJEnable);

            VideoMenu.DropDownItems.Add(new ToolStripSeparator());  // --------------------------

            ToolStripMenuItem WindowingEnable = new ToolStripMenuItem("Windowing", null,
                new EventHandler((object sender, EventArgs e) => {
                    this.gba.ppu.ExternalWindowingEnable = (((ToolStripMenuItem)sender).Checked ^= true);
                }));
            WindowingEnable.Checked = true;
            VideoMenu.DropDownItems.Add(WindowingEnable);

            ToolStripMenuItem BlendingEnable = new ToolStripMenuItem("Blending", null,
                new EventHandler((object sender, EventArgs e) => {
                    this.gba.ppu.ExternalBlendingEnable = (((ToolStripMenuItem)sender).Checked ^= true);
                }));
            BlendingEnable.Checked = true;
            VideoMenu.DropDownItems.Add(BlendingEnable);


            // Assign the ToolStripMenuItem that displays 
            // the list of child forms.
            ms.MdiWindowListItem = GameMenu;

            // Add the window ToolStripMenuItem to the MenuStrip.
            ms.Items.Add(GameMenu);
            ms.Items.Add(EmulationMenu);
            ms.Items.Add(VideoMenu);

            // Dock the MenuStrip to the top of the form.
            ms.Dock = DockStyle.Top;

            // The Form.MainMenuStrip property determines the merge target.
            this.MainMenuStrip = ms;

            // Add the MenuStrip last.
            // This is important for correct placement in the z-order.
            this.Controls.Add(ms);
        }
    }
}
