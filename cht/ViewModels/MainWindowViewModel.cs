using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using cht.Classes;

namespace offsets
{
    //Need to check offsets when csgo updates
    public static class netvars
    {
        // client.dll
        public const Int32 dwLocalPlayer = 0xDB35DC;
        public const Int32 dwEntityList = 0x4DCEEAC;
        public const Int32 dwClientState = 0x58CFC4;

        // player
        public const Int32 dwGlowObjectManager = 0x5317308;
        public const Int32 m_iGlowIndex = 0x10488;

        // entity
        public const Int32 teamNum = 0xF4;
        public const Int32 spotted = 0x93D;
    }
}

namespace cht.ViewModels
{

    public class MainWindowViewModel : ViewModelBase
    {
        public ICommand StartCommand { get; }

        private string processStatus = "";
        public string ProcessStatus
        {
            get => processStatus;
            set => this.RaiseAndSetIfChanged(ref processStatus, value);
        }

        private string clientStatus = "";
        public string ClientStatus
        {
            get => clientStatus;
            set => this.RaiseAndSetIfChanged(ref clientStatus, value);
        }

        private bool isEnabled;
        public bool IsEnable
        {
            get => isEnabled;
            set => this.RaiseAndSetIfChanged(ref isEnabled, value);
        }

        public ICommand AppExit { get; private set; }

        public MainWindowViewModel()
        {

            isEnabled = true;

            AppExit = ReactiveCommand.Create(() => {
                Environment.Exit(0);
            });

            StartCommand = ReactiveCommand.Create(() =>
            {
                Start();
            });

        }

        struct Color
        {
            public float r;
            public float g;
            public float b;
            public float a;
            public Color(float r, float g, float b, float a)
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }

        }


        private async void Start()
        {
            var process = Process.GetProcessesByName("csgo")[0];

            if (process != null)
                ProcessStatus = "Process: csgo.exe running!";
            else
                ProcessStatus = "Process: please start csgo.exe";

            var processHandle = Win32API.OpenProcess(Win32API.PROCESS_VM_READ | Win32API.PROCESS_VM_WRITE, false, process.Id);
            int clientAddress = 0;

            await Task.Delay(1000);
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName == "client.dll")
                {
                    clientAddress = (int)module.BaseAddress;
                    ClientStatus = $"Client: client.dll loaded! Address: {clientAddress}";
                }

            }
            var color = new Color(1.0f, 0f, 0f, 1f);
            isEnabled = false;

            while (true)
            {

                var localPlayer = Memory.ReadMemory<int>((int)processHandle, clientAddress + offsets.netvars.dwLocalPlayer);

                //if (localPlayer == 0)
                //    continue;

                var glowObjectManager = Memory.ReadMemory<int>((int)processHandle, clientAddress + offsets.netvars.dwGlowObjectManager);


                for (int i = 1; i <= 32; i++)
                {

                    //RADAR

                    //var playerAddr = Memory.ReadMemory<int>((int)processHandle, clientAddress + offsets.netvars.dwEntityList + (i * 0x10));
                    //Memory.WriteMemory<int>((int)processHandle, playerAddr + offsets.netvars.spotted, 1);

                    // player count on the server! 0x10 size of 1 player
                    var entity = Memory.ReadMemory<int>((int)processHandle, clientAddress + offsets.netvars.dwEntityList + i * 0x10);
                    if (entity == 0)
                        continue;

                    // dont glow if they are on our team
                    if (Memory.ReadMemory<int>((int)processHandle, entity + offsets.netvars.teamNum) == Memory.ReadMemory<int>((int)processHandle, localPlayer + offsets.netvars.teamNum))
                        continue;

                    var glowIndex = Memory.ReadMemory<int>((int)processHandle, entity + offsets.netvars.m_iGlowIndex);

                    //hard way
                    Memory.WriteMemory<int>((int)processHandle, glowObjectManager + (glowIndex * 0x38) + 0x8, 1.0f); //red
                    Memory.WriteMemory<int>((int)processHandle, glowObjectManager + (glowIndex * 0x38) + 0xC, 0f); //green
                    Memory.WriteMemory<int>((int)processHandle, glowObjectManager + (glowIndex * 0x38) + 0x10, 0f); //blue
                    Memory.WriteMemory<int>((int)processHandle, glowObjectManager + (glowIndex * 0x38) + 0x14, 1.0f); //alpha

                    //simplified
                    //Memory.WriteMemory<int>((int)processHandle, glowObjectManager + (glowIndex * 0x38) + 0x8, color);

                    Memory.WriteMemory<bool>((int)processHandle, glowObjectManager + (glowIndex * 0x38) + 0x28, true);
                    Memory.WriteMemory<bool>((int)processHandle, glowObjectManager + (glowIndex * 0x38) + 0x29, false);

                }


                await Task.Delay(1); //necessery to not run this shit billion times
            }

        }


        //public static KeyValuePair<string,IntPtr> ClientAddress;
        //private async void Start()
        //{


        //    if (!ProcOpen)
        //    {
        //        await Task.Delay(1000);
        //        return;
        //    }
        //    ProcessStatus = "Process: csgo.exe running!";
        //    await Task.Delay(1000);

        //    var modules = memory.mProc.Modules;
        //    foreach (var item in modules)
        //    {
        //        if (item.Key == "client.dll")
        //        {
        //            ClientAddress = item;
        //            ClientStatus = $"Client: client.dll loaded! Address: {ClientAddress.Value}";
        //        }

        //    }

        //    //cht Part

        //    while(true)
        //    {
        //        UIntPtr client = (UIntPtr)(long)ClientAddress.Value;

        //        UIntPtr localPlayerAddress = client + offsets.netvars.dwLocalPlayer;
        //        var localPlayer = (UIntPtr)(long)memory.ReadUIntPtr(localPlayerAddress);
        //        if (localPlayer == UIntPtr.Zero) //check if localplayer Address exists
        //            continue;

        //        UIntPtr localTeamAddress = client + offsets.netvars.teamNum;
        //        var localTeam = (UIntPtr)(long)memory.ReadUIntPtr(localTeamAddress);

        //        var glowObjectManager = (UIntPtr)(long)memory.ReadUIntPtr(client + offsets.netvars.dwGlowObjectManager);

        //        for (int i = 1; i <= 32; i++)
        //        {
        //            var entity = memory.ReadUIntPtr(client + offsets.netvars.dwEntityList + i * 0x10); // player count on the server!


        //            //don't glow if they are in our team
        //            if (memory.ReadUIntPtr((UIntPtr)(long)entity + offsets.netvars.teamNum) == memory.ReadUIntPtr((UIntPtr)(long)localPlayer + offsets.netvars.teamNum))
        //                continue;

        //            var glowIndex = (Int32)memory.ReadUIntPtr((UIntPtr)(Int32)entity + offsets.netvars.m_iGlowIndex);

        //            string glowObjectManagerString = "0x60fb64b0";

        //            //var asd = glwobjManager;
        //            //var res = glowObjectManager.ToString();
        //            //var r = res;

        //            memory.WriteMemory((glowObjectManagerString + (glowIndex * 0x38) + 0x8).ToString(), "float", "1.f");
        //        }




        //        await Task.Delay(2);
        //    }





        //}




    }
}
