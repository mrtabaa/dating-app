import { PaginationParams } from "./paginationParams";
import { FollowPredicate } from "./follow-predicate";

export class FollowParams extends PaginationParams {
    predicate = FollowPredicate.followings;
}