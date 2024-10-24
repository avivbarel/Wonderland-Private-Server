﻿using Plugin;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wonderland_Private_Server {
    public partial class Form1 : Form {
        bool blockclose = true;

        PluginManager phostManager;


        public Form1() {

            InitializeComponent();
        }


        void DebugSystem_onNewLog(object sender, DebugItem j) {
            new Task(() => {
                this.Invoke(new Action(() => {
                    switch (j.Type) {
                        case DebugItemType.Info_Light: SystemLog.AppendText(j.Msg + "\r\n=============================\r\n"); break;
                        case DebugItemType.Network_Light: NetWorkLog.AppendText(j.Msg + "\r\n=============================\r\n"); break;
                        case DebugItemType.Error: errorLog.AppendText(j.Msg + "\r\n=============================\r\n"); break;
                            //case Utilities.LogType.DB: errorLog.AppendText(j.Msg + "\r\n=============================\r\n"); break;
                            //case Utilities.LogType.THRD: SystemLog.AppendText(j.Msg + "\r\n=============================\r\n"); break;
                            //case Utilities.LogType.UPDT: SystemLog.AppendText(j.Msg + "\r\n=============================\r\n"); break;
                    }
                }));
            }
            ).Start();
        }

        private void Form1_Load(object sender, EventArgs e) {
            Thread MainThread = new Thread(new ThreadStart(MainThreadWork));
            MainThread.IsBackground = true;
            MainThread.Init();
        }

        void GuiThread() {
            do {
                #region LogGUI
                string s = null;
                //if (!string.IsNullOrEmpty((s = DebugSystem.PullLogItem())))
                //    SystemLog.AppendText(s);
                #endregion
                #region TaskGui

                if (cGlobal.ApplicationTasks != null)
                    this.BeginInvoke(new Action(() => { cGlobal.ApplicationTasks.onUpdateGuiTick(); }));

                #endregion
                #region Form.System.Status gui
                try {
                    this.BeginInvoke(new Action(() => {
                        //thrd_label.Text = string.Format("Thread Cnt - {0}", ThreadManager.Count);
                    }));
                } catch { }
                #endregion
                #region Form.Update
                //try
                //{
                //    this.BeginInvoke(new Action(() =>
                //    {
                //        DateTime tmp = (cGlobal.ApplicationTasks.TaskItems.Count(c => c.TaskName == "Updating Application") > 0) ? cGlobal.ApplicationTasks.TaskItems.Single(c => c.TaskName == "Updating Application").Createdat : new DateTime();

                //        autoUpdt_label.Text = string.Format(tmp.Add(cGlobal.SrvSettings.Update.AutoUpdt_Schedule).ToString());
                //    }));
                //}
                //catch { }
                #endregion
                Thread.Sleep(5);
            }
            while (cGlobal.Run);
        }


        void MainThreadWork() {

            cGlobal.Run = true;
            DebugSystem.Initialize(ref MainOutput, true);
            DebugSystem.VerboseLvl = 1;

            DebugSystem.Write("[Init] - Initializing DataFile Objects");
            Console.WriteLine("[Init] - Initializing DataFile Objects");
            cGlobal.ItemDatManager = new DataFiles.PhxItemDat();
            cGlobal.ItemDatManager.Load(Environment.CurrentDirectory + "\\Data\\itemDat.wpdat");
            DebugSystem.Write("[Init] - Initializing DataBase Objects");
            cGlobal.gUserDataBase = new DataBase.UserDataBase();
            cGlobal.gCharacterDataBase = new DataBase.CharacterDataBase();
            cGlobal.gCharacterDataBase.ItemDat = cGlobal.ItemDatManager;
            cGlobal.gGameDataBase = new DataBase.GameDataBase();
            cGlobal.gGameDataBase.ItemDat = cGlobal.ItemDatManager;
            DebugSystem.Write("[Init] - Intializing Systems Please Wait.....");
            cGlobal.ApplicationTasks = new Server.TaskManager();
            cGlobal.Update_System = new Server.System.UpdateSystem();
            cGlobal.Update_System.MainFrm = this;
            cGlobal.Update_System.MapUpdtPanel = UpdtPane2;
            cGlobal.Update_System.AppUpdtPanel = UpdatePane;
            phostManager = new PluginManager();
            phostManager.Intialize();

            cGlobal.gLoginServer = new Server.LoginServer();
            cGlobal.gWorld = new Server.WorldServer(phostManager);

            //cGlobal.WLO_World = new Server.WloWorldNode();
            //cGlobal.gCharacterDataBase = new DataManagement.DataBase.CharacterDataBase();
            //cGlobal.gEveManager = new DataManagement.DataFiles.EveManager();
            //cGlobal.gGameDataBase = new DataManagement.DataBase.GameDataBase();
            //cGlobal.gItemManager = new DataManagement.DataFiles.ItemManager();
            //cGlobal.gSkillManager = new DataManagement.DataFiles.SkillDataFile();
            //cGlobal.gCompoundDat = new DataManagement.DataFiles.cCompound2Dat();
            //cGlobal.gUserDataBase = new UserDataBase();
            //cGlobal.gNpcManager = new DataManagement.DataFiles.NpcDat();

            cGlobal.SrvSettings = new Server.Config.Settings();

            #region load settings file

            DebugSystem.Write("Loading Settings File");
            if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PServer\\Config.settings.wlo")) {
                System.Xml.Serialization.XmlSerializer diskio = new System.Xml.Serialization.XmlSerializer(typeof(Server.Config.Settings));

                try {
                    using (StreamReader file = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PServer\\Config.settings.wlo"))
                        cGlobal.SrvSettings = (Server.Config.Settings)diskio.Deserialize(file);
                    DebugSystem.Write("Settings File loaded successfully");
                } catch { DebugSystem.Write("Settings File failed to load"); }
            } else
                DebugSystem.Write("Settings File not found");



            cGlobal.gUserDataBase.TableName = cGlobal.SrvSettings.DB.TableName_Ref;
            cGlobal.gUserDataBase.Username_Ref = cGlobal.SrvSettings.DB.Username_Ref;
            cGlobal.gUserDataBase.Password_Ref = cGlobal.SrvSettings.DB.Password_Ref;
            cGlobal.gUserDataBase.DataBaseID_Ref = cGlobal.SrvSettings.DB.UserID_Ref;
            cGlobal.gUserDataBase.IM_Ref = cGlobal.SrvSettings.DB.IM_Ref;
            cGlobal.gUserDataBase.CharacterID1_Ref = cGlobal.SrvSettings.DB.CharacterID1_Ref;
            cGlobal.gUserDataBase.CharacterID2_Ref = cGlobal.SrvSettings.DB.CharacterID2_Ref;
            cGlobal.gUserDataBase.Char_Delete_Code_Ref = cGlobal.SrvSettings.DB.Char_Delete_Code_Ref;
            cGlobal.gUserDataBase.PassVerification = (Game.VerifyPassType)cGlobal.SrvSettings.DB.PassVerification;
            //if (GitUptOption.SelectedIndex != (byte)cGlobal.SrvSettings.Update.UpdtControl)
            //    GitUptOption.SelectedIndex = (byte)cGlobal.SrvSettings.Update.UpdtControl;



            #endregion



            //use for testing
#if DEBUG

#endif


            #region Intialize Base Threads
            DebugSystem.Write("Intializing Gui Thread");
            Thread tmp2 = new Thread(new ThreadStart(GuiThread));
            tmp2.Name = "Gui Thread";
            tmp2.Init();

            #endregion

            #region intial check  if theres an update from github
            DebugSystem.Write("Checking For Update on Git...");

            //if (cGlobal.SrvSettings.Update.UpdtControl != Server.Config.UpdtSetting.Never && cGlobal.ApplicationTasks.TaskItems.Count(c => c.TaskName == "Updating Application") > 0)
            //    goto ShutDwn;
            #endregion


            #region Configure Form Data
            this.Invoke(new Action(() => {
                dataGridView1.Columns[0].DataPropertyName = "TaskName";
                dataGridView1.Columns[1].DataPropertyName = "Interval";
                dataGridView1.Columns[2].DataPropertyName = "LastExecution";
                dataGridView1.Columns[3].DataPropertyName = "NextExecution";
                dataGridView1.Columns[4].DataPropertyName = "Status";
                dataGridView1.DataSource = cGlobal.ApplicationTasks.TaskItems;
            }));

            TableName.DataBindings.Add("Text", cGlobal.SrvSettings.DB, "TableName_Ref");
            Username_Ref.DataBindings.Add("Text", cGlobal.SrvSettings.DB, "Username_Ref");
            Password_Ref.DataBindings.Add("Text", cGlobal.SrvSettings.DB, "Password_Ref");
            UserID_Ref.DataBindings.Add("Text", cGlobal.SrvSettings.DB, "UserID_Ref");
            IM_Ref.DataBindings.Add("Text", cGlobal.SrvSettings.DB, "IM_Ref");
            Char_Delete_Code_Ref.DataBindings.Add("Text", cGlobal.SrvSettings.DB, "Char_Delete_Code_Ref");
            _passVerifi.DataBindings.Add("SelectedIndex", cGlobal.SrvSettings.DB, "PassVerification");
            #endregion

            #region DataBase Initialization


            DebugSystem.Write("Testing Connection to UserDatabase");
            try {
                if (cGlobal.gUserDataBase.TestConnection())
                    DebugSystem.Write("Connection Successful");
                else
                    DebugSystem.Write("Connection not successful\r\n unable to authencate  users connecting to server");

                DebugSystem.Write("Testing Connection to Character Database");
                if (cGlobal.gCharacterDataBase.TestConnection()) {
                    DebugSystem.Write("Connection Successful");
                    DebugSystem.Write("Verifying DataBase Tables");
                    cGlobal.gCharacterDataBase.VerifySetup();
                } else
                    DebugSystem.Write("Connection not successful\r\n unable to create neccesary tables for the server");
            } catch (Exception e) { DebugSystem.Write(new ExceptionData(e)); }


            #endregion

            #region Load Data Files
            //cGlobal.gItemManager.LoadItems("Data\\Item.dat");
            //cGlobal.gSkillManager.LoadSkills("Data\\Skill.dat");
            //cGlobal.gNpcManager.LoadNpc("Data\\Npc.dat");
            //cGlobal.gEveManager.LoadFile("Data\\eve.Emg");
            //cGlobal.gCompoundDat.Load("Data\\Compound.dat");
            //cGlobal.gCompoundDat.Load("Data\\Compound2.dat", false);

            #endregion

            DebugSystem.Write("[Init] - Intializing Server Please Wait.....");
            #region Initialize Server Components
            cGlobal.gWorld.Initialize();
            cGlobal.gLoginServer.Initialize();

            //cGlobal.WLO_World.Initialize();
            Thread.Sleep(2);
            //cGlobal.TcpListener.Initialize();
            #endregion


            do {
                #region Thread Management
                //try
                //{
                //    Thread r;
                //    foreach (var t in ThreadManager. cGlobal.ThreadManager)
                //        if (!t.Value.IsAlive)
                //            if (cGlobal.ThreadManager.TryRemove(t.Key, out r))
                //                DebugSystem.Write(t.Value.Name + " has been Terminated", Utilities.LogType.THRD);
                //}
                //catch { }
                #endregion
                #region TaskManager
                cGlobal.ApplicationTasks.onUpdateTick();
                #endregion
                Thread.Sleep(10);
            }
            while (cGlobal.Run);

            ShutDown();



        }


        public void ShutDown() {
            this.Invoke(new Action(() => { this.Enabled = false; }));

            UI.ShutDown_Dialog tmp = new UI.ShutDown_Dialog();
            tmp.Location = this.Location;
            tmp.Left = this.Left + 100;
            tmp.Top = this.Top + 250;

            tmp.ShowDialog();
            tmp.Dispose();

            cGlobal.Run = false;
            blockclose = false;


            this.Invoke(new Action(() => { Close(); }));
        }


        #region Git Client Events

        void GitClient_GitinfoUpdated(object sender, EventArgs e) {
            try {
                //this.BeginInvoke(new Action(() =>
                //{
                //    UpdatePane.Controls.Clear();

                //    foreach (var y in cGlobal.GitClient.Releases.Where(c => c.TagName != null).OrderByDescending(c => new Version(c.TagName)))
                //        UpdatePane.Controls.Add(new Gui.Update.GitUpdateItem(cGlobal.GitClient.myVersion, y));

                //}));
            } catch (Exception fe) { DebugSystem.Write(new ExceptionData(fe)); }
        }
        #endregion

        #region Form Events
        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            if (cGlobal.Run || blockclose)
                e.Cancel = true;
            cGlobal.Run = false;
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex < 0 || (e.ColumnIndex != dataGridView1.Columns["retrytask"].Index)) return;


            if (e.ColumnIndex == dataGridView1.Columns["retrytask"].Index)
                cGlobal.ApplicationTasks.TaskItems[e.RowIndex].onRetry();
        }

        #region Update Section
        private void GitBranch_TextChanged(object sender, EventArgs e) {
            //if (cGlobal.GitClient != null && cGlobal.GitClient.Branch != GitBranch.Text)
            //{
            //    cGlobal.SrvSettings.Update.GitBranch = GitBranch.Text;
            //    cGlobal.GitClient.Branch = GitBranch.Text;
            //    cGlobal.GitClient.CheckFor_Update();
            //}
        }
        private void GitUptOption_SelectedIndexChanged(object sender, EventArgs e) {
            //if (cGlobal.SrvSettings.Update.UpdtControl != (Server.Config.UpdtSetting)GitUptOption.SelectedIndex)
            //    cGlobal.SrvSettings.Update.UpdtControl = (Server.Config.UpdtSetting)GitUptOption.SelectedIndex;

            //if ((Server.Config.UpdtSetting)GitUptOption.SelectedIndex == Server.Config.UpdtSetting.Never)
            //    cGlobal.ApplicationTasks.EndTask("Application Update");
            //else if ((Server.Config.UpdtSetting)GitUptOption.SelectedIndex != Server.Config.UpdtSetting.Never)
            //    cGlobal.ApplicationTasks.CreateTask("Application Update", cGlobal.SrvSettings.Update.UpdtChk_Interval);
        }
        #endregion

        private void updtrefresh_ValueChanged(object sender, EventArgs e) {
            //if (cGlobal.SrvSettings.Update.UpdtChk_Interval.Minutes != updtrefresh.Value)
            //    cGlobal.SrvSettings.Update.UpdtChk_Interval = new TimeSpan(0,(int)updtrefresh.Value,0);

            //if ((Server.Config.UpdtSetting)GitUptOption.SelectedIndex != Server.Config.UpdtSetting.Never)
            //    cGlobal.ApplicationTasks.ChangeInterval("Application Update", cGlobal.SrvSettings.Update.UpdtChk_Interval);
        }
        private void autoUpdt_Hr_ValueChanged(object sender, EventArgs e) {
            //cGlobal.SrvSettings.Update.AutoUpdt_Schedule = new TimeSpan((int)autoUpdt_Hr.Value, (int)autoUpdt_Min.Value, 0);
        }
        private void autoUpdt_Min_ValueChanged(object sender, EventArgs e) {
            //cGlobal.SrvSettings.Update.AutoUpdt_Schedule = new TimeSpan((int)autoUpdt_Hr.Value, (int)autoUpdt_Min.Value, 0);
        }

        #endregion

        private void updtschedule_CheckedChanged(object sender, EventArgs e) {
            cGlobal.SrvSettings.Update.EnableSchedUpdate = updtschedule.Checked;
        }

        private void updtday_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void updttime_ValueChanged(object sender, EventArgs e) {

        }

        private void updttime2_ValueChanged(object sender, EventArgs e) {

        }

        private void updt_warn_how_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void updt_warn_CheckedChanged(object sender, EventArgs e) {
            cGlobal.SrvSettings.Update.WarnofUpdate = updt_warn.Checked;
        }





    }
}
