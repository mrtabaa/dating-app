import { BaseParams } from "./BaseParams";
import { FollowPredicate } from "./follow-predicate";

export class FollowParams extends BaseParams {
    predicate = FollowPredicate.followings;
}