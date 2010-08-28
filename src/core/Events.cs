using System;
using System.Collections.Generic;
using System.Text;

namespace BlackLight.Services.Core
{
    public partial class ServicesCore : IRC
    {
        public class ServicesEvents
        {
            public delegate void onPingEventHandler();
            internal onPingEventHandler onPingEvent;

            public event onPingEventHandler onPing
            {
                add
                {
                    onPingEvent = (onPingEventHandler)System.Delegate.Combine(onPingEvent, value);
                }
                remove
                {
                    onPingEvent = (onPingEventHandler)System.Delegate.Remove(onPingEvent, value);
                }
            }

            public delegate void LogMessageEventHandler(string Origin, string Method, BlackLight.Services.Error.Errors Type, string Message, string Extra, string Exception, string Trace);
            internal LogMessageEventHandler LogMessageEvent;

            public event LogMessageEventHandler LogMessage
            {
                add
                {
                    LogMessageEvent = (LogMessageEventHandler)System.Delegate.Combine(LogMessageEvent, value);
                }
                remove
                {
                    LogMessageEvent = (LogMessageEventHandler)System.Delegate.Remove(LogMessageEvent, value);
                }
            }

            public delegate void onPrivMsgEventHandler(BlackLight.Services.Nodes.Client Source, string Dest, string[] Message);
            internal onPrivMsgEventHandler onPrivMsgEvent;

            public event onPrivMsgEventHandler onPrivMsg
            {
                add
                {
                    onPrivMsgEvent = (onPrivMsgEventHandler)System.Delegate.Combine(onPrivMsgEvent, value);
                }
                remove
                {
                    onPrivMsgEvent = (onPrivMsgEventHandler)System.Delegate.Remove(onPrivMsgEvent, value);
                }
            }

            public delegate void onNoticeEventHandler(BlackLight.Services.Nodes.Client Source, string Dest, string[] Message);
            internal onNoticeEventHandler onNoticeEvent;

            public event onNoticeEventHandler onNotice
            {
                add
                {
                    onNoticeEvent = (onNoticeEventHandler)System.Delegate.Combine(onNoticeEvent, value);
                }
                remove
                {
                    onNoticeEvent = (onNoticeEventHandler)System.Delegate.Remove(onNoticeEvent, value);
                }
            }

            public delegate void onSNoticeEventHandler(string server, string who, string message);
            internal onSNoticeEventHandler onSNoticeEvent;

            public event onSNoticeEventHandler onSNotice
            {
                add
                {
                    onSNoticeEvent = (onSNoticeEventHandler)System.Delegate.Combine(onSNoticeEvent, value);
                }
                remove
                {
                    onSNoticeEvent = (onSNoticeEventHandler)System.Delegate.Remove(onSNoticeEvent, value);
                }
            }

            public delegate void onCTCPEventHandler();
            internal onCTCPEventHandler onCTCPEvent;

            public event onCTCPEventHandler onCTCP
            {
                add
                {
                    onCTCPEvent = (onCTCPEventHandler)System.Delegate.Combine(onCTCPEvent, value);
                }
                remove
                {
                    onCTCPEvent = (onCTCPEventHandler)System.Delegate.Remove(onCTCPEvent, value);
                }
            }

            public delegate void onJoinEventHandler(string NickName, string Channel);
            internal onJoinEventHandler onJoinEvent;

            public event onJoinEventHandler onJoin
            {
                add
                {
                    onJoinEvent = (onJoinEventHandler)System.Delegate.Combine(onJoinEvent, value);
                }
                remove
                {
                    onJoinEvent = (onJoinEventHandler)System.Delegate.Remove(onJoinEvent, value);
                }
            }

            public delegate void onPartEventHandler(string NickName, string Channel);
            internal onPartEventHandler onPartEvent;

            public event onPartEventHandler onPart
            {
                add
                {
                    onPartEvent = (onPartEventHandler)System.Delegate.Combine(onPartEvent, value);
                }
                remove
                {
                    onPartEvent = (onPartEventHandler)System.Delegate.Remove(onPartEvent, value);
                }
            }

            public delegate void onQuitEventHandler(string NickName, string Reason);
            internal onQuitEventHandler onQuitEvent;

            public event onQuitEventHandler onQuit
            {
                add
                {
                    onQuitEvent = (onQuitEventHandler)System.Delegate.Combine(onQuitEvent, value);
                }
                remove
                {
                    onQuitEvent = (onQuitEventHandler)System.Delegate.Remove(onQuitEvent, value);
                }
            }

            public delegate void onSVSKillEventHandler(string NickName, string Reason);
            internal onSVSKillEventHandler onSVSKillEvent;

            public event onSVSKillEventHandler onSVSKill
            {
                add
                {
                    onSVSKillEvent = (onSVSKillEventHandler)System.Delegate.Combine(onSVSKillEvent, value);
                }
                remove
                {
                    onSVSKillEvent = (onSVSKillEventHandler)System.Delegate.Remove(onSVSKillEvent, value);
                }
            }

            public delegate void onKickEventHandler(string Kicker, string Kicked, string Reason);
            internal onKickEventHandler onKickEvent;

            public event onKickEventHandler onKick
            {
                add
                {
                    onKickEvent = (onKickEventHandler)System.Delegate.Combine(onKickEvent, value);
                }
                remove
                {
                    onKickEvent = (onKickEventHandler)System.Delegate.Remove(onKickEvent, value);
                }
            }

            public delegate void OnFinishedNetBurstEventHandler();
            internal OnFinishedNetBurstEventHandler OnFinishedNetBurstEvent;

            public event OnFinishedNetBurstEventHandler OnFinishedNetBurst
            {
                add
                {
                    OnFinishedNetBurstEvent = (OnFinishedNetBurstEventHandler)System.Delegate.Combine(OnFinishedNetBurstEvent, value);
                }
                remove
                {
                    OnFinishedNetBurstEvent = (OnFinishedNetBurstEventHandler)System.Delegate.Remove(OnFinishedNetBurstEvent, value);
                }
            }

            public delegate void onOpEventHandler();
            internal onOpEventHandler onOpEvent;

            public event onOpEventHandler onOp
            {
                add
                {
                    onOpEvent = (onOpEventHandler)System.Delegate.Combine(onOpEvent, value);
                }
                remove
                {
                    onOpEvent = (onOpEventHandler)System.Delegate.Remove(onOpEvent, value);
                }
            }

            public delegate void onDeOpEventHandler();
            internal onDeOpEventHandler onDeOpEvent;

            public event onDeOpEventHandler onDeOp
            {
                add
                {
                    onDeOpEvent = (onDeOpEventHandler)System.Delegate.Combine(onDeOpEvent, value);
                }
                remove
                {
                    onDeOpEvent = (onDeOpEventHandler)System.Delegate.Remove(onDeOpEvent, value);
                }
            }

            public delegate void onOwnerEventHandler();
            internal onOwnerEventHandler onOwnerEvent;

            public event onOwnerEventHandler onOwner
            {
                add
                {
                    onOwnerEvent = (onOwnerEventHandler)System.Delegate.Combine(onOwnerEvent, value);
                }
                remove
                {
                    onOwnerEvent = (onOwnerEventHandler)System.Delegate.Remove(onOwnerEvent, value);
                }
            }

            public delegate void onDeOwnerEventHandler();
            internal onDeOwnerEventHandler onDeOwnerEvent;

            public event onDeOwnerEventHandler onDeOwner
            {
                add
                {
                    onDeOwnerEvent = (onDeOwnerEventHandler)System.Delegate.Combine(onDeOwnerEvent, value);
                }
                remove
                {
                    onDeOwnerEvent = (onDeOwnerEventHandler)System.Delegate.Remove(onDeOwnerEvent, value);
                }
            }

            public delegate void onHalfOpEventHandler();
            internal onHalfOpEventHandler onHalfOpEvent;

            public event onHalfOpEventHandler onHalfOp
            {
                add
                {
                    onHalfOpEvent = (onHalfOpEventHandler)System.Delegate.Combine(onHalfOpEvent, value);
                }
                remove
                {
                    onHalfOpEvent = (onHalfOpEventHandler)System.Delegate.Remove(onHalfOpEvent, value);
                }
            }

            public delegate void onDeHalfOpEventHandler();
            internal onDeHalfOpEventHandler onDeHalfOpEvent;

            public event onDeHalfOpEventHandler onDeHalfOp
            {
                add
                {
                    onDeHalfOpEvent = (onDeHalfOpEventHandler)System.Delegate.Combine(onDeHalfOpEvent, value);
                }
                remove
                {
                    onDeHalfOpEvent = (onDeHalfOpEventHandler)System.Delegate.Remove(onDeHalfOpEvent, value);
                }
            }

            public delegate void onOnProtectEventHandler();
            internal onOnProtectEventHandler onOnProtectEvent;

            public event onOnProtectEventHandler onOnProtect
            {
                add
                {
                    onOnProtectEvent = (onOnProtectEventHandler)System.Delegate.Combine(onOnProtectEvent, value);
                }
                remove
                {
                    onOnProtectEvent = (onOnProtectEventHandler)System.Delegate.Remove(onOnProtectEvent, value);
                }
            }

            public delegate void onDeProtectEventHandler();
            internal onDeProtectEventHandler onDeProtectEvent;

            public event onDeProtectEventHandler onDeProtect
            {
                add
                {
                    onDeProtectEvent = (onDeProtectEventHandler)System.Delegate.Combine(onDeProtectEvent, value);
                }
                remove
                {
                    onDeProtectEvent = (onDeProtectEventHandler)System.Delegate.Remove(onDeProtectEvent, value);
                }
            }

            public delegate void onVoiceEventHandler();
            internal onVoiceEventHandler onVoiceEvent;

            public event onVoiceEventHandler onVoice
            {
                add
                {
                    onVoiceEvent = (onVoiceEventHandler)System.Delegate.Combine(onVoiceEvent, value);
                }
                remove
                {
                    onVoiceEvent = (onVoiceEventHandler)System.Delegate.Remove(onVoiceEvent, value);
                }
            }

            public delegate void onDeVoiceEventHandler();
            internal onDeVoiceEventHandler onDeVoiceEvent;

            public event onDeVoiceEventHandler onDeVoice
            {
                add
                {
                    onDeVoiceEvent = (onDeVoiceEventHandler)System.Delegate.Combine(onDeVoiceEvent, value);
                }
                remove
                {
                    onDeVoiceEvent = (onDeVoiceEventHandler)System.Delegate.Remove(onDeVoiceEvent, value);
                }
            }

            public delegate void onModeEventHandler();
            internal onModeEventHandler onModeEvent;

            public event onModeEventHandler onMode
            {
                add
                {
                    onModeEvent = (onModeEventHandler)System.Delegate.Combine(onModeEvent, value);
                }
                remove
                {
                    onModeEvent = (onModeEventHandler)System.Delegate.Remove(onModeEvent, value);
                }
            }

            public delegate void onBanEventHandler();
            internal onBanEventHandler onBanEvent;

            public event onBanEventHandler onBan
            {
                add
                {
                    onBanEvent = (onBanEventHandler)System.Delegate.Combine(onBanEvent, value);
                }
                remove
                {
                    onBanEvent = (onBanEventHandler)System.Delegate.Remove(onBanEvent, value);
                }
            }

            public delegate void onUnbanEventHandler();
            internal onUnbanEventHandler onUnbanEvent;

            public event onUnbanEventHandler onUnban
            {
                add
                {
                    onUnbanEvent = (onUnbanEventHandler)System.Delegate.Combine(onUnbanEvent, value);
                }
                remove
                {
                    onUnbanEvent = (onUnbanEventHandler)System.Delegate.Remove(onUnbanEvent, value);
                }
            }

            public delegate void onExceptEventHandler();
            internal onExceptEventHandler onExceptEvent;

            public event onExceptEventHandler onExcept
            {
                add
                {
                    onExceptEvent = (onExceptEventHandler)System.Delegate.Combine(onExceptEvent, value);
                }
                remove
                {
                    onExceptEvent = (onExceptEventHandler)System.Delegate.Remove(onExceptEvent, value);
                }
            }

            public delegate void onUnExceptEventHandler();
            internal onUnExceptEventHandler onUnExceptEvent;

            public event onUnExceptEventHandler onUnExcept
            {
                add
                {
                    onUnExceptEvent = (onUnExceptEventHandler)System.Delegate.Combine(onUnExceptEvent, value);
                }
                remove
                {
                    onUnExceptEvent = (onUnExceptEventHandler)System.Delegate.Remove(onUnExceptEvent, value);
                }
            }

            public delegate void onNickEventHandler(string Oldnick, string Newnick);
            internal onNickEventHandler onNickEvent;

            public event onNickEventHandler onNick
            {
                add
                {
                    onNickEvent = (onNickEventHandler)System.Delegate.Combine(onNickEvent, value);
                }
                remove
                {
                    onNickEvent = (onNickEventHandler)System.Delegate.Remove(onNickEvent, value);
                }
            }

            public delegate void onClientConnectEventHandler(string nickname, BlackLight.Services.Nodes.Client data);
            internal onClientConnectEventHandler onClientConnectEvent;

            public event onClientConnectEventHandler onClientConnect
            {
                add
                {
                    onClientConnectEvent = (onClientConnectEventHandler)System.Delegate.Combine(onClientConnectEvent, value);
                }
                remove
                {
                    onClientConnectEvent = (onClientConnectEventHandler)System.Delegate.Remove(onClientConnectEvent, value);
                }
            }

            public delegate void onClientDisconnectEventHandler();
            internal onClientDisconnectEventHandler onClientDisconnectEvent;

            public event onClientDisconnectEventHandler onClientDisconnect
            {
                add
                {
                    onClientDisconnectEvent = (onClientDisconnectEventHandler)System.Delegate.Combine(onClientDisconnectEvent, value);
                }
                remove
                {
                    onClientDisconnectEvent = (onClientDisconnectEventHandler)System.Delegate.Remove(onClientDisconnectEvent, value);
                }
            }

            public delegate void onServerEventHandler(string name, BlackLight.Services.Nodes.Server data);
            internal onServerEventHandler onServerEvent;

            public event onServerEventHandler onServer
            {
                add
                {
                    onServerEvent = (onServerEventHandler)System.Delegate.Combine(onServerEvent, value);
                }
                remove
                {
                    onServerEvent = (onServerEventHandler)System.Delegate.Remove(onServerEvent, value);
                }
            }

            public delegate void onSQUITEventHandler(string name, string reason);
            internal onSQUITEventHandler onSQUITEvent;

            public event onSQUITEventHandler onSQUIT
            {
                add
                {
                    onSQUITEvent = (onSQUITEventHandler)System.Delegate.Combine(onSQUITEvent, value);
                }
                remove
                {
                    onSQUITEvent = (onSQUITEventHandler)System.Delegate.Remove(onSQUITEvent, value);
                }
            }

            public delegate void onKillEventHandler(string NickName, string Reason);
            internal onKillEventHandler onKillEvent;

            public event onKillEventHandler onKill
            {
                add
                {
                    onKillEvent = (onKillEventHandler)System.Delegate.Combine(onKillEvent, value);
                }
                remove
                {
                    onKillEvent = (onKillEventHandler)System.Delegate.Remove(onKillEvent, value);
                }
            }

            // Public Event onIRCError(ByVal message As String)
            //  Public Event onCmd(ByVal message As String)
            //  Public Event onDebug(ByVal message As String)
            //    Public Event onUnknown(ByVal message As String)
            public delegate void onTopicChangeEventHandler(string channel, BlackLight.Services.Nodes.IRCUser Source, string topic);
            internal onTopicChangeEventHandler onTopicChangeEvent;

            public event onTopicChangeEventHandler onTopicChange
            {
                add
                {
                    onTopicChangeEvent = (onTopicChangeEventHandler)System.Delegate.Combine(onTopicChangeEvent, value);
                }
                remove
                {
                    onTopicChangeEvent = (onTopicChangeEventHandler)System.Delegate.Remove(onTopicChangeEvent, value);
                }
            }

            public delegate void onTopicEventHandler(string channel, string topic);
            internal onTopicEventHandler onTopicEvent;

            public event onTopicEventHandler onTopic
            {
                add
                {
                    onTopicEvent = (onTopicEventHandler)System.Delegate.Combine(onTopicEvent, value);
                }
                remove
                {
                    onTopicEvent = (onTopicEventHandler)System.Delegate.Remove(onTopicEvent, value);
                }
            }

            public delegate void onTopicChannelWhoTimeEventHandler(string channel, string who, int time);
            internal onTopicChannelWhoTimeEventHandler onTopicChannelWhoTimeEvent;

            public event onTopicChannelWhoTimeEventHandler onTopicChannelWhoTime
            {
                add
                {
                    onTopicChannelWhoTimeEvent = (onTopicChannelWhoTimeEventHandler)System.Delegate.Combine(onTopicChannelWhoTimeEvent, value);
                }
                remove
                {
                    onTopicChannelWhoTimeEvent = (onTopicChannelWhoTimeEventHandler)System.Delegate.Remove(onTopicChannelWhoTimeEvent, value);
                }
            }
        }
    }
}