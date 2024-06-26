using System;
using System.Runtime.InteropServices;
using UnityEngine.Serialization;

namespace UnityGGPO {

    public enum GGPOPlayerType {
        GGPO_PLAYERTYPE_LOCAL,
        GGPO_PLAYERTYPE_REMOTE,
        GGPO_PLAYERTYPE_SPECTATOR,
    }

    [Serializable]
    public struct GGPOPlayer {
        public GGPOPlayerType type;
        public int playerNum;
        public string ipAddress;
        public ushort port;
        public ulong steamId;

        public override string ToString() {
            return $"({type},{playerNum},{ipAddress},{port},{steamId})";
        }
    }

    public class GGPONetworkStats {
        public int sendQueueLen;
        public int recvQueueLen;
        public int ping;
        public int kbpsSent;
        public int localFramesBehind;
        public int remoteFramesBehind;
    }

    public static partial class GGPO {
        private const string libraryName = "UnityGGPO";

        public const int MAX_PLAYERS = 4;
        public const int MAX_PREDICTION_FRAMES = 8;
        public const int MAX_SPECTATORS = 32;

        public const int OK = 0;
        public const int INVALID_HANDLE = -1;

        public const int ERRORCODE_SUCCESS = 0;
        public const int ERRORCODE_GENERAL_FAILURE = -1;
        public const int ERRORCODE_INVALID_SESSION = 1;
        public const int ERRORCODE_INVALID_PLAYER_HANDLE = 2;
        public const int ERRORCODE_PLAYER_OUT_OF_RANGE = 3;
        public const int ERRORCODE_PREDICTION_THRESHOLD = 4;
        public const int ERRORCODE_UNSUPPORTED = 5;
        public const int ERRORCODE_NOT_SYNCHRONIZED = 6;
        public const int ERRORCODE_IN_ROLLBACK = 7;
        public const int ERRORCODE_INPUT_DROPPED = 8;
        public const int ERRORCODE_PLAYER_DISCONNECTED = 9;
        public const int ERRORCODE_TOO_MANY_SPECTATORS = 10;
        public const int ERRORCODE_INVALID_REQUEST = 11;

        public const int EVENTCODE_CONNECTED_TO_PEER = 1000;
        public const int EVENTCODE_SYNCHRONIZING_WITH_PEER = 1001;
        public const int EVENTCODE_SYNCHRONIZED_WITH_PEER = 1002;
        public const int EVENTCODE_RUNNING = 1003;
        public const int EVENTCODE_DISCONNECTED_FROM_PEER = 1004;
        public const int EVENTCODE_TIMESYNC = 1005;
        public const int EVENTCODE_CONNECTION_INTERRUPTED = 1006;
        public const int EVENTCODE_CONNECTION_RESUMED = 1007;

        public static bool SUCCEEDED(int result) {
            return result == ERRORCODE_SUCCESS;
        }

        public static string GetErrorCodeMessage(int result) {
            switch (result) {
                case ERRORCODE_SUCCESS:
                case ERRORCODE_GENERAL_FAILURE:
                case ERRORCODE_INVALID_SESSION:
                case ERRORCODE_INVALID_PLAYER_HANDLE:
                case ERRORCODE_PLAYER_OUT_OF_RANGE:
                case ERRORCODE_PREDICTION_THRESHOLD:
                case ERRORCODE_UNSUPPORTED:
                case ERRORCODE_NOT_SYNCHRONIZED:
                case ERRORCODE_IN_ROLLBACK:
                case ERRORCODE_INPUT_DROPPED:
                case ERRORCODE_PLAYER_DISCONNECTED:
                case ERRORCODE_TOO_MANY_SPECTATORS:
                case ERRORCODE_INVALID_REQUEST:
                    return result.ToString();
                default:
                    return "INVALID_ERRORCODE";
            }
        }

        public static string Version {
            get {
                return Utils.GetString(UggPluginVersion());
            }
        }

        public static int BuildNumber {
            get {
                return UggPluginBuildNumber();
            }
        }

        public delegate void LogDelegate(string text);

        public delegate bool BeginGameDelegate(string text);

        public delegate bool AdvanceFrameDelegate(int flags);

        public unsafe delegate bool LoadGameStateDelegate(void* buffer, int length);

        public unsafe delegate bool LogGameStateDelegate(string filename, void* buffer, int length);

        public unsafe delegate bool SaveGameStateDelegate(void** buffer, int* len, int* checksum, int frame);

        public unsafe delegate void FreeBufferDelegate(void* buffer);

        public delegate bool OnEventDelegate(IntPtr evt);

        [DllImport(libraryName, CharSet = CharSet.Ansi)]
        private static extern IntPtr UggPluginVersion();

        [DllImport(libraryName)]
        private static extern int UggPluginBuildNumber();

        [DllImport(libraryName)]
        private static extern void UggSetLogDelegate(IntPtr callback);

        [DllImport(libraryName)]
        public static extern int UggTestStartSession(out IntPtr session,
            IntPtr beginGame,
            IntPtr advanceFrame,
            IntPtr loadGameState,
            IntPtr logGameState,
            IntPtr saveGameState,
            IntPtr freeBuffer,
            IntPtr onEvent,
            string game, int num_players, int localport);

        [DllImport(libraryName)]
        private static extern int UggStartSession(out IntPtr session,
            IntPtr beginGame,
            IntPtr advanceFrame,
            IntPtr loadGameState,
            IntPtr logGameState,
            IntPtr saveGameState,
            IntPtr freeBuffer,
            IntPtr onEvent,
            string game, int num_players, int localport);

        [DllImport(libraryName)]
        private static extern int UggStartSyncTest(out IntPtr session,
            IntPtr beginGame,
            IntPtr advanceFrame,
            IntPtr loadGameState,
            IntPtr logGameState,
            IntPtr saveGameState,
            IntPtr freeBuffer,
            IntPtr onEvent,
            string game, int num_players, int frames);

        [DllImport(libraryName)]
        private static extern int UggStartSpectating(out IntPtr session,
            IntPtr beginGame,
            IntPtr advanceFrame,
            IntPtr loadGameState,
            IntPtr logGameState,
            IntPtr saveGameState,
            IntPtr freeBuffer,
            IntPtr onEvent,
            string game, int num_players, int localport, string host_ip, int host_port);

        [DllImport(libraryName)]
        private static extern int UggSetDisconnectNotifyStart(IntPtr ggpo, int timeout);

        [DllImport(libraryName)]
        private static extern int UggSetDisconnectTimeout(IntPtr ggpo, int timeout);

        [DllImport(libraryName)]
        private static extern int UggSynchronizeInput(IntPtr ggpo, IntPtr inputs, int length, out int disconnect_flags);

        [DllImport(libraryName)]
        private static extern int UggAddLocalInput(IntPtr ggpo, int local_player_handle, long input);

        [DllImport(libraryName)]
        private static extern int UggCloseSession(IntPtr ggpo);

        [DllImport(libraryName)]
        private static extern int UggIdle(IntPtr ggpo, int timeout);

        [DllImport(libraryName)]
        private static extern int UggAddPlayer(IntPtr ggpo, int player_type, int player_num, string player_ip_address, ushort player_port, ulong steam_id, out int phandle);

        [DllImport(libraryName)]
        private static extern int UggDisconnectPlayer(IntPtr ggpo, int phandle);

        [DllImport(libraryName)]
        private static extern int UggSetFrameDelay(IntPtr ggpo, int phandle, int frame_delay);

        [DllImport(libraryName)]
        private static extern int UggAdvanceFrame(IntPtr ggpo);

        [DllImport(libraryName)]
        private static extern void UggLog(IntPtr ggpo, string text);

        [DllImport(libraryName)]
        private static extern void UggSetSteamLegacyMessages(IntPtr ggpo, bool legacy);

        [DllImport(libraryName)]
        private static extern int UggGetNetworkStats(IntPtr ggpo, int phandle,
            out int send_queue_len,
            out int recv_queue_len,
            out int ping,
            out int kbps_sent,
            out int local_frames_behind,
            out int remote_frames_behind);

        // Access

        private static IntPtr _logDelegate;

        public static void SetLogDelegate(LogDelegate callback) {
            _logDelegate = callback != null ? Marshal.GetFunctionPointerForDelegate(callback) : IntPtr.Zero;
            UggSetLogDelegate(_logDelegate);
        }

        public static int StartSession(out IntPtr session,
                IntPtr beginGame,
                IntPtr advanceFrame,
                IntPtr loadGameState,
                IntPtr logGameState,
                IntPtr saveGameState,
                IntPtr freeBuffer,
                IntPtr onEvent,
                string game, int num_players, int localport) {
            return UggStartSession(out session, beginGame, advanceFrame, loadGameState, logGameState, saveGameState, freeBuffer, onEvent, game, num_players, localport);
        }

        public static int StartSyncTest(out IntPtr session,
                IntPtr beginGame,
                IntPtr advanceFrame,
                IntPtr loadGameState,
                IntPtr logGameState,
                IntPtr saveGameState,
                IntPtr freeBuffer,
                IntPtr onEvent,
                string game, int num_players, int frames) {
            return UggStartSyncTest(out session, beginGame, advanceFrame, loadGameState, logGameState, saveGameState, freeBuffer, onEvent, game, num_players, frames);
        }

        public static int StartSpectating(out IntPtr session,
                IntPtr beginGame,
                IntPtr advanceFrame,
                IntPtr loadGameState,
                IntPtr logGameState,
                IntPtr saveGameState,
                IntPtr freeBuffer,
                IntPtr onEvent,
                string game, int num_players, int localport, string host_ip, int host_port) {
            return UggStartSpectating(out session, beginGame, advanceFrame, loadGameState, logGameState, saveGameState, freeBuffer, onEvent, game, num_players, localport, host_ip, host_port);
        }

        public static int SetDisconnectNotifyStart(IntPtr ggpo, int timeout) {
            return UggSetDisconnectNotifyStart(ggpo, timeout);
        }

        public static int SetDisconnectTimeout(IntPtr ggpo, int timeout) {
            return UggSetDisconnectTimeout(ggpo, timeout);
        }

        public static void SynchronizeInput(IntPtr ggpo, long[] inputs, out int disconnect_flags) {
            var length = inputs.Length; 
            
            for (var i = 0; i < length; i++)
                inputs[i] = default;
            
            IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(long)) * length);
            var result = UggSynchronizeInput(ggpo, pnt, length, out disconnect_flags);
            Marshal.Copy(pnt, inputs, 0, length);
            Marshal.FreeHGlobal(pnt);
            if (!SUCCEEDED(result)) {
                throw new Exception(GetErrorCodeMessage(result));
            }
        }

        public static int AddLocalInput(IntPtr ggpo, int local_player_handle, long input) {
            return UggAddLocalInput(ggpo, local_player_handle, input);
        }

        public static int CloseSession(IntPtr ggpo) {
            return UggCloseSession(ggpo);
        }

        public static int Idle(IntPtr ggpo, int timeout) {
            return UggIdle(ggpo, timeout);
        }

        public static int AddPlayer(IntPtr ggpo, int player_type, int player_num, string player_ip_address, ushort player_port, ulong steam_id, out int phandle) {
            return UggAddPlayer(ggpo, player_type, player_num, player_ip_address, player_port, steam_id, out phandle);
        }

        public static int DisconnectPlayer(IntPtr ggpo, int phandle) {
            return UggDisconnectPlayer(ggpo, phandle);
        }

        public static int SetFrameDelay(IntPtr ggpo, int phandle, int frame_delay) {
            return UggSetFrameDelay(ggpo, phandle, frame_delay);
        }

        public static int AdvanceFrame(IntPtr ggpo) {
            return UggAdvanceFrame(ggpo);
        }

        public static void Log(IntPtr ggpo, string text) {
            UggLog(ggpo, text);
        }

        public static void SetSteamLegacyMessages(IntPtr ggpo, bool legacy) {
            UggSetSteamLegacyMessages(ggpo, legacy);
        }

        public static int GetNetworkStats(IntPtr ggpo, int phandle,
                out int sendQueueLen,
                out int recvQueueLen,
                out int ping,
                out int kbpsSent,
                out int localFramesBehind,
                out int remoteFramesBehind) {
            return UggGetNetworkStats(ggpo, phandle, out sendQueueLen, out recvQueueLen, out ping, out kbpsSent, out localFramesBehind, out remoteFramesBehind);
        }

        public static int TestStartSession(out IntPtr session,
                IntPtr beginGame,
                IntPtr advanceFrame,
                IntPtr loadGameState,
                IntPtr logGameState,
                IntPtr saveGameState,
                IntPtr freeBuffer,
                IntPtr onEvent,
                string game, int num_players, int localport) {
            return UggTestStartSession(out session, beginGame, advanceFrame, loadGameState, logGameState, saveGameState, freeBuffer, onEvent, game, num_players, localport);
        }
    }
}