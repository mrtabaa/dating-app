import { Member } from "../member.model";

export interface FollowModifiedEmit {
    member: Member;
    isFollowing: boolean;
}