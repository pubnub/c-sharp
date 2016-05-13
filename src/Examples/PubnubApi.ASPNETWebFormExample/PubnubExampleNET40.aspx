<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PubnubExampleNET40.aspx.cs"
    Inherits="PubNubMessaging.PubnubExampleNET40" %>

<%@ Register TagPrefix="asp" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>PubNub Demo Example for ASP.NET (NET 4.0/4.5)</title>
    <style type="text/css">
        body
        {
            font-family: Verdana;
            font-size: small;
            margin: 0;
        }
        table.mainContainer
        {
            border: 1px #000000 solid;
            vertical-align: top;
        }
        
        col.mainContainerColgroupCol1
        {
            width: 10%;
        }
        col.mainContainerColgroupCol1
        {
            width: 90%*;
        }
        
        
        table td, table th
        {
            padding: 3px;
        }
        
        #divMessage
        {
            float: left;
            margin-left: 5px;
            width: 99%;
            height: 300px;
            background-color: #000000;
            border: 1px #000000 solid;
            overflow: auto;
            font: Verdana, Geneva, sans-serif;
            color: Aqua;
            font-size: small;
        }
        
        .SmallButton
        {
            height: 16px;
            padding: 0px;
            font-size: x-small;
            vertical-align: middle;
        }
        
        .ModalPopupBG
        {
            background-color: #F0F0F0;
            filter: alpha(opacity=50);
            opacity: 0.7;
        }
        
        .publishPopup
        {
            min-width: 400px;
            min-height: 330px;
            background: #FFFF99;
            border: 1px #FFFF99 solid;
            margin: 5px;
        }
        
        .PopupHeader
        {
            background-color: #000000;
            color: #ffffff;
            height: 30px;
            font: Verdana, Geneva, sans-serif;
            font-size: large;
            text-indent: 5px;
            vertical-align: middle;
            text-align: left;
            padding: 5px;
            margin: 0px;
        }
        
        .PopupBody
        {
            padding: 5px;
        }
        
        .PopupControls
        {
            padding: 5px;
            text-align:right;
        }
    </style>
    <script type="text/javascript">
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" EnablePartialRendering="true" runat="server">
    </asp:ScriptManager>
    <div>
        <table class="mainContainer">
            <colgroup>
                <col class="mainContainerColgroupCol1" />
                <col class="mainContainerColgroupCol2" />
            </colgroup>
            <tr>
                <td valign="top">
                    <asp:UpdatePanel ID="UpdatePanelLeft" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="btnReset" />
                            <asp:AsyncPostBackTrigger ControlID="btnTime" />
                            <asp:AsyncPostBackTrigger ControlID="btnSubscribe" />
                            <asp:AsyncPostBackTrigger ControlID="btnPresence" />
                            <asp:AsyncPostBackTrigger ControlID="btnDetailedHistory" />
                            <asp:AsyncPostBackTrigger ControlID="btnUnsubscribe" />
                            <asp:AsyncPostBackTrigger ControlID="btnPresenceUnsub" />
                            <asp:AsyncPostBackTrigger ControlID="btnDisableNetwork" />
                            <asp:AsyncPostBackTrigger ControlID="btnEnableNetwork" />
                            <asp:AsyncPostBackTrigger ControlID="btnDisconnectAndRetry" />
                        </Triggers>
                        <ContentTemplate>
                            <fieldset>
                                <table border="0" cellpadding="0" cellspacing="0">
                                    <tr>
                                        <td>
                                            <table border="0" cellpadding="0" cellspacing="0">
                                                <tr>
                                                    <td valign="top">
                                                        <table width="100%" border="0" cellpadding="0" cellspacing="0">
                                                            <tr>
                                                                <td align="right">
                                                                    <asp:Button ID="btnReset" runat="server" Text="Reset Pubnub Instance" Enabled="false"
                                                                        CssClass="SmallButton" OnClick="btnReset_Click" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:CheckBox ID="chkSSL" runat="server" Text="Enable SSL" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:CheckBox ID="chkResumeOnReconnect" runat="server" Text="Resume On Reconnect"
                                                                        Checked="true" />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td valign="top">
                                                        <table border="0" cellpadding="0" cellspacing="0">
                                                            <tr>
                                                                <td>
                                                                    Origin
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="txtOrigin" runat="server" Text="pubsub.pubnub.com" Width="150"
                                                                        AutoPostBack="false" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    Publish Key
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="txtPubKey" runat="server" Text="demo-36" Width="150" AutoPostBack="false" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    Subscriber Key
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="txtSubKey" runat="server" Text="demo-36" Width="150" AutoPostBack="false" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    Cipher Key
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="txtCipher" runat="server" Text="" Width="150" AutoPostBack="false" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    Secret Key
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="txtSecret" runat="server" Text="demo-36" Width="150" AutoPostBack="false" />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <table>
                                                            <tr>
                                                                <td>
                                                                    Subscriber Timeout (in sec)
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="txtSubscribeTimeout" runat="server" Text="310" Width="50" AutoPostBack="false"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    Non Subscribe Timeout (in sec)
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="txtNonSubscribeTimeout" runat="server" Text="15" Width="50" AutoPostBack="false"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    Number of MAX retries
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="txtNetworkMaxRetries" runat="server" Text="50" Width="50" AutoPostBack="false"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    Retry Interval (in sec)
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="txtRetryInterval" runat="server" Text="10" Width="50" AutoPostBack="false"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    Local Client Heartbeat Interval (in sec)
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="txtLocalClientHeartbeatInterval" runat="server" Text="10" Width="50"
                                                                        AutoPostBack="false"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    Presence Heartbeat(in sec)
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="txtPresenceHeartbeat" runat="server" Text="63" Width="50" AutoPostBack="false"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    Presence Heartbeat Interval(in sec)
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="txtPresenceHeartbeatInterval" runat="server" Text="60" Width="50"
                                                                        AutoPostBack="false"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td valign="top">
                                            <table>
                                                <tr>
                                                    <td colspan="2">
                                                        UUID:
                                                        <asp:TextBox ID="txtUUID" runat="server" Text="" Width="220" AutoPostBack="false" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        Channel Name :
                                                        <asp:TextBox ID="txtChannel" runat="server" Text="my_hello_world" AutoPostBack="false"></asp:TextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        Channel Group :
                                                        <asp:TextBox ID="txtChannelGroup" runat="server" Text="my_hello_group" AutoPostBack="false"></asp:TextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        Auth Key (for non-PAM methods)
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="txtAuthKey" runat="server" Text="" Width="150" AutoPostBack="false" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        Auth Key (for PAM methods)
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="txtPAMAuthKey" runat="server" Text="" Width="150" AutoPostBack="false" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:Button ID="btnPresence" runat="server" Text="Presence" Width="138px" CommandName="Presence"
                                                            OnCommand="btnPresence_Command" />
                                                    </td>
                                                    <td>
                                                        <asp:Button ID="btnSubscribe" runat="server" Text="Subscribe" Width="138px" CommandName="Subscribe"
                                                            OnCommand="btnSubscribe_Command" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:Button ID="btnDetailedHistory" runat="server" Text="Detailed History" Width="138px"
                                                            CommandName="DetailedHistory" OnCommand="btnDetailedHistory_Command" />
                                                    </td>
                                                    <td>
                                                        <asp:Button ID="btnPublish" runat="server" Text="Publish" Width="138px" CommandName="Publish"
                                                            OnCommand="btnPublish_Command" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:Button ID="btnUnsubscribe" runat="server" Text="Unsubscribe" Width="138px" CommandName="Unsubscribe"
                                                            OnCommand="btnUnsubscribe_Command" />
                                                    </td>
                                                    <td>
                                                        <asp:Button ID="btnHereNow" runat="server" Text="Here Now" Width="138px" CommandName="HereNow"
                                                            OnCommand="btnHereNow_Command" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:Button ID="btnPresenceUnsub" runat="server" Text="Presence-Unsub" Width="138px"
                                                            CommandName="PresenceUnsubscribe" OnCommand="btnPresenceUnsub_Command" />
                                                    </td>
                                                    <td>
                                                        <asp:Button ID="btnTime" runat="server" Text="Time" Width="138px" CommandName="Time"
                                                            OnCommand="btnTime_Command" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:Button ID="btnGrant" runat="server" Text="Grant Channel" Width="138px" CommandName="GrantAccess"
                                                            OnCommand="btnGrant_Command" />
                                                    </td>
                                                    <td>
                                                        <asp:Button ID="btnRevoke" runat="server" Text="Revoke Channel" Width="138px" CommandName="RevokeAccess"
                                                            OnCommand="btnRevoke_Command" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:Button ID="btnAudit" runat="server" Text="Audit Channnel" Width="138px" CommandName="AuditAccess"
                                                            OnCommand="btnAudit_Command" />
                                                    </td>
                                                    <td>
                                                        <asp:Button ID="btnDisconnectAndRetry" runat="server" Text="Disconnect/Retry" Width="138px"
                                                            CommandName="DisconnectAndRetry" OnCommand="btnDisconnectAndRetry_Command" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:Button ID="btnDisableNetwork" runat="server" Text="Disable Network" Width="138px"
                                                            CommandName="DisableNetwork" OnCommand="btnDisableNetwork_Command" />
                                                    </td>
                                                    <td>
                                                        <asp:Button ID="btnEnableNetwork" runat="server" Text="Enable Network" Width="138px"
                                                            CommandName="EnableNetwork" OnCommand="btnEnableNetwork_Command" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:Button ID="btnWhereNow" runat="server" Text="Where Now" Width="138px" CommandName="WhereNow"
                                                            OnCommand="btnWhereNow_Command" />
                                                    </td>
                                                    <td>
                                                        <asp:Button ID="btnGlobalHereNow" runat="server" Text="Global Here Now" Width="138px"
                                                            CommandName="GlobalHereNow" OnCommand="btnGlobalHereNow_Command" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:Button ID="btnSetUserState" runat="server" Text="Set User State" Width="138px" CommandName="SetUserState"
                                                            OnCommand="btnSetUserState_Command" />
                                                    </td>
                                                    <td>
                                                        <asp:Button ID="btnGetUserState" runat="server" Text="Get User State" Width="138px"
                                                            CommandName="GetUserState" OnCommand="btnGetUserState_Command" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:Button ID="btnChannelGroup" runat="server" Text="Channel Group" Width="138px" CommandName="ChannelGroup"
                                                            OnCommand="btnChannelGroup_Command" />
                                                    </td>
                                                    <td>
                                                        
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        <asp:ModalPopupExtender ID="publishModalPopupExtender" runat="server" TargetControlID="btnPublish"
                                                            PopupDragHandleControlID="PopupHeader" Drag="true" PopupControlID="popUpPanelPublishMessage"
                                                            CancelControlID="btnCancel" BackgroundCssClass="ModalPopupBG" />
                                                        <asp:Panel ID="popUpPanelPublishMessage" runat="server" Style="display: none">
                                                            <div class="publishPopup">
                                                                <div class="PopupHeader" id="PopupHeader">
                                                                    Enter message to publish:</div>
                                                                <div class="PopupBody">
                                                                    <p>
                                                                        <asp:RadioButton ID="radNormalPublish" GroupName="gPublish" Checked="true" Text="Normal" runat="server"/><br />
                                                                        <asp:CheckBox ID="chkStoreInHistory" Text="Store In History" Checked="true"  runat="server"/>
                                                                        <asp:TextBox ID="txtPublishMessage" TextMode="MultiLine" runat="server" Height="180px"
                                                                            Width="98%" Wrap="true"></asp:TextBox>

                                                                        <asp:RadioButton ID="radToastPublish" GroupName="gPublish" Text="Send Toast" runat="server"/><br />
                                                                        <asp:RadioButton ID="radFlipTilePublish" GroupName="gPublish" Text="Send Flip Tile" runat="server"/><br />
                                                                        <asp:RadioButton ID="radCycleTilePublish" GroupName="gPublish" Text="Send Cycle Tile" runat="server"/><br />
                                                                        <asp:RadioButton ID="radIconicTilePublish" GroupName="gPublish" Text="Send Iconic Tile" runat="server"/><br />

                                                                        <asp:Label ID="lblErrorMessage" Text="" runat="server" Font-Names="verdana" ForeColor="Red"
                                                                            Height="15px" Font-Size="X-Small"></asp:Label>
                                                                    </p>
                                                                </div>
                                                                <div class="PopupControls">
                                                                    <asp:Button ID="btnOkay" Text="Publish" runat="server" OnClick="btnOkay_OnClick" />
                                                                    <input id="btnCancel" type="button" value="Cancel" />
                                                                </div>
                                                            </div>
                                                        </asp:Panel>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        <asp:ModalPopupExtender ID="globalHereNowPopupExtender" runat="server" TargetControlID="btnGlobalHereNow"
                                                            PopupDragHandleControlID="globalHereNowPopup" Drag="true" PopupControlID="popupPanelGlobalHereNow"
                                                            CancelControlID="btnCancelGlobalHereNow" BackgroundCssClass="ModalPopupBG">
                                                        </asp:ModalPopupExtender>
                                                        <asp:Panel ID="popupPanelGlobalHereNow" runat="server" Style="display: none">
                                                            <div class="publishPopup">
                                                                <div class="PopupHeader" id="globalHereNowPopup">
                                                                    Select the options for Global Here Now:</div>
                                                                <div class="PopupBody">
                                                                    <p>
                                                                        <asp:CheckBox ID="chbShowUUIDList" Text="Show UUID List" Checked="true" runat="server" />
                                                                        <br />
                                                                        <asp:CheckBox ID="chbShowUserState" Text="Show User State" runat="server" />
                                                                    </p>
                                                                </div>
                                                                <div class="PopupControls">
                                                                    <asp:Button ID="btnOkayGlobalHereNow" Text="Global Here Now" runat="server" OnClick="btnOkayGlobalHereNow_OnClick" />
                                                                    <input id="btnCancelGlobalHereNow" type="button" value="Cancel" />
                                                                </div>
                                                            </div>
                                                        </asp:Panel>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        <asp:ModalPopupExtender ID="hereNowPopupExtender" runat="server" TargetControlID="btnHereNow"
                                                            PopupDragHandleControlID="hereNowPopup" Drag="true" PopupControlID="popupPanelHereNow"
                                                            CancelControlID="btnCancelHereNow" BackgroundCssClass="ModalPopupBG">
                                                        </asp:ModalPopupExtender>
                                                        <asp:Panel ID="popupPanelHereNow" runat="server" Style="display: none">
                                                            <div class="publishPopup">
                                                                <div class="PopupHeader" id="hereNowPopup">
                                                                    Select the options for Here Now:</div>
                                                                <div class="PopupBody">
                                                                    <p>
                                                                        <asp:CheckBox ID="chbShowUUIDList2" Text="Show UUID List" Checked="true" runat="server" />
                                                                        <br />
                                                                        <asp:CheckBox ID="chbShowUserState2" Text="Show User State" runat="server" />
                                                                    </p>
                                                                </div>
                                                                <div class="PopupControls">
                                                                    <asp:Button ID="btnOkayHereNow" Text="Here Now" runat="server" OnClick="btnOkayHereNow_OnClick" />
                                                                    <input id="btnCancelHereNow" type="button" value="Cancel" />
                                                                </div>
                                                            </div>
                                                        </asp:Panel>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        <asp:ModalPopupExtender ID="setUserStatePopupExtender" runat="server" TargetControlID="btnSetUserState"
                                                            PopupDragHandleControlID="setUserStatePopup" Drag="true" PopupControlID="popupPanelSetUserState"
                                                            CancelControlID="btnCancelSetUserState" BackgroundCssClass="ModalPopupBG">
                                                        </asp:ModalPopupExtender>
                                                        <asp:Panel ID="popupPanelSetUserState" runat="server" Style="display: none">
                                                            <div class="publishPopup">
                                                                <div class="PopupHeader" id="setUserStatePopup">
                                                                    Set User State:</div>
                                                                <div class="PopupBody">
                                                                    <p>
                                                                        Enter user state in json format ( Eg. {"key1":"value1","key2":"value2"}  ):
                                                                        <br />
                                                                        <asp:TextBox ID="txtJsonUserState" Text="" Width="300px" runat="server"></asp:TextBox>
                                                                    </p>
                                                                </div>
                                                                <div class="PopupControls">
                                                                    <asp:Button ID="btnOkayJsonSetUserState" Text="Set User State in Json Format" runat="server" OnClick="btnOkayJsonSetUserState_OnClick" />
                                                                    <input id="btnCancelJsonSetUserState" type="button" value="Cancel" />
                                                                </div>
                                                                <p style="text-align:center; font-weight:bold"> OR </p>                                                                
                                                                <div class="PopupBody">
                                                                    <p>
                                                                        Enter user state in dictionary format (key:value pair):
                                                                        <br />
                                                                        <asp:TextBox ID="txtKey1" Text="" Width="100px" runat="server"></asp:TextBox> : <asp:TextBox ID="txtValue1" Text="" Width="100px" runat="server"></asp:TextBox><br />
                                                                    </p>
                                                                </div>
                                                                <div class="PopupControls">
                                                                    <asp:Button ID="btnOkaySetUserState" Text="Set User State" runat="server" OnClick="btnOkaySetUserState_OnClick" />
                                                                    <input id="btnCancelSetUserState" type="button" value="Cancel" />
                                                                </div>

                                                                
                                                            </div>
                                                        </asp:Panel>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        <asp:ModalPopupExtender ID="getUserStatePopupExtender" runat="server" TargetControlID="btnGetUserState"
                                                            PopupDragHandleControlID="getUserStatePopup" Drag="true" PopupControlID="popupPanelGetUserState"
                                                            CancelControlID="btnCancelGetUserState" BackgroundCssClass="ModalPopupBG">
                                                        </asp:ModalPopupExtender>
                                                        <asp:Panel ID="popupPanelGetUserState" runat="server" Style="display: none">
                                                            <div class="publishPopup">
                                                                <div class="PopupHeader" id="getUserStatePopup">
                                                                    Get User State:</div>
                                                                <div class="PopupBody">
                                                                    <p>
                                                                        Enter UUID (optional):
                                                                        <br />
                                                                        <asp:TextBox ID="txtGetUserStateUUID" Text="" Width="300px" runat="server"></asp:TextBox>
                                                                    </p>
                                                                </div>
                                                                <div class="PopupControls">
                                                                    <asp:Button ID="btnOkayGetUserState" Text="Get User State" runat="server" OnClick="btnOkayGetUserState_OnClick" />
                                                                    <input id="btnCancelGetUserState" type="button" value="Cancel" />
                                                                </div>

                                                            </div>
                                                        </asp:Panel>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        <asp:ModalPopupExtender ID="channelGroupPopupExtender" runat="server" TargetControlID="btnChannelGroup"
                                                            PopupDragHandleControlID="channelGroupPopup" Drag="true" PopupControlID="popupPanelChannelGroup"
                                                            CancelControlID="btnCancelChannelGroup" BackgroundCssClass="ModalPopupBG">
                                                        </asp:ModalPopupExtender>
                                                        <asp:Panel ID="popupPanelChannelGroup" runat="server" Style="display: none">
                                                            <div class="publishPopup">
                                                                <div class="PopupHeader" id="channelGroupPopup">
                                                                    ChannelGroup: Get/Add/Remove channels</div>
                                                                <div class="PopupBody">
                                                                    <p>
                                                                        Channel Group Name:
                                                                        <br />
                                                                        <asp:TextBox ID="txtChannelGroupName" Text="my_hello_group" Width="300px" runat="server"></asp:TextBox>
                                                                    </p>
                                                                    <p>
                                                                        <asp:RadioButton ID="radChannelGroupGet" GroupName="gChannelGroup" Text="Get Channel List" runat="server"/><br />
                                                                    </p>
                                                                    <p>
                                                                        <asp:RadioButton ID="radChannelGroupGrant" GroupName="gChannelGroup" Text="PAM Grant" runat="server"/><br />
                                                                    </p>
                                                                    <p>
                                                                        <asp:RadioButton ID="radChannelGroupAudit" GroupName="gChannelGroup" Text="PAM Audit" runat="server"/><br />
                                                                    </p>
                                                                    <p>
                                                                        <asp:RadioButton ID="radChannelGroupRevoke" GroupName="gChannelGroup" Text="PAM Revoke" runat="server"/><br />
                                                                    </p>
                                                                    <p>
                                                                        <asp:RadioButton ID="radChannelGroupAdd" GroupName="gChannelGroup" Text="Add Channel [for multiple channels use comma delimiter]" runat="server"/><br />
                                                                        Channel(s):
                                                                        <asp:TextBox ID="txtChannelGroupAddChannels" Text="" Width="300px" runat="server"></asp:TextBox><br />
                                                                    </p>
                                                                    <p>
                                                                        <asp:RadioButton ID="radChannelGroupRemove" GroupName="gChannelGroup" Text="Remove Channel[for multiple channels use comma delimiter]" runat="server"/><br />
                                                                        Channel(s):
                                                                        <asp:TextBox ID="txtChannelGroupRemoveChannels" Text="" Width="300px" runat="server"></asp:TextBox><br />
                                                                    </p>
                                                                </div>
                                                                <div class="PopupControls">
                                                                    <asp:Button ID="btnOkayChannelGroup" Text="Submit" runat="server" OnClick="btnOkayChannelGroup_OnClick" />
                                                                    <input id="btnCancelChannelGroup" type="button" value="Cancel" />
                                                                </div>

                                                            </div>
                                                        </asp:Panel>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </fieldset>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>
                <td valign="top">
                    <asp:UpdatePanel ID="UpdatePanelRight" runat="server" ChildrenAsTriggers="false"
                        UpdateMode="Conditional">
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="txtMessage" />
                            <asp:AsyncPostBackTrigger ControlID="UpdateTimer" EventName="Tick" />
                        </Triggers>
                        <ContentTemplate>
                            <fieldset>
                                <asp:TextBox ID="txtMessage" runat="server" ReadOnly="true" BackColor="Black" ForeColor="Aqua"
                                    Height="500px" TextMode="MultiLine" Wrap="true" AutoPostBack="false" Width="100%"></asp:TextBox>
                                <asp:Timer runat="server" ID="UpdateTimer" Interval="500" Enabled="true" OnTick="UpdateTimer_Tick" />
                            </fieldset>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>
            </tr>
        </table>
        <script type="text/javascript" language="javascript">

            var xPos, yPos;
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_pageLoaded(PageLoadedEventHandler);
            function PageLoadedEventHandler() {
                //alert("page loaded event handler");
            }

            prm.add_beginRequest(BeginRequestEventHandler);
            function BeginRequestEventHandler() {
                if ($get('<%= txtMessage.ClientID %>') != null) {
                    xPos = $get('<%= txtMessage.ClientID %>').scrollLeft;
                    yPos = $get('<%= txtMessage.ClientID %>').scrollTop;
                }
                //xPos = $get('txtMessage').scrollLeft;
                //yPos = $get('txtMessage').scrollTop;
                //alert("begin request event handler");
            }

            prm.add_endRequest(EndRequestEventHandler);
            function EndRequestEventHandler() {
                if ($get('<%= txtMessage.ClientID %>') != null) {
                    $get('<%= txtMessage.ClientID %>').scrollLeft = xPos;
                    $get('<%= txtMessage.ClientID %>').scrollTop = $get('<%= txtMessage.ClientID %>').scrollHeight;
                }
                //$get('txtMessage').scrollLeft = xPos;
                //$get('txtMessage').scrollTop = yPos;
                //alert("end request event handler");
            }

            prm.add_initializeRequest(InitializeRequestEventHandler);
            function InitializeRequestEventHandler() {
                //alert("Initialize Request Event Handler");
            }

            prm.add_pageLoading(PageLoadingEventHandler);
            function PageLoadingEventHandler() {
                //alert("Page Loading Event Handler");
            }

            function MessageDisplayHolderMouseOver() {
                //var updateTimer = $find('<%= UpdateTimer.ClientID %>');
                //updateTimer.set_enabled(false);
                //updateTimer._stopTimer();
            }

            function MessageDisplayHolderMouseOut() {
                //var updateTimer = $find('<%= UpdateTimer.ClientID %>');
                //updateTimer.set_enabled(true);
                //updateTimer._startTimer();
            }

        </script>
        <br />
    </div>
    </form>
</body>
</html>
