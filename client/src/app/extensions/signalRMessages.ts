export class SignalRMessages {
  // PresenceHub
  static readonly GetOnlineUsers = 'GetOnlineUsers';

  // MessageHub
  static readonly JoinGroup = 'JoinGroup';
  static readonly NotifyMembersOnJoined = 'NotifyMembersOnJoined';
  static readonly UpdatedReadOn = 'UpdatedReadOn';
  static readonly Create = 'Create';
  static readonly NewMessageRes = 'NewMessageRes';
  static readonly SendingError = 'SendingError';
  static readonly LeaveGroup = 'LeaveGroup';
}
