import {PaginationParams} from "./paginationParams";
import {FollowPredicate} from "../../enums/follow-predicate.enum";

export class FollowParams extends PaginationParams {
  predicate = FollowPredicate.FOLLOWINGS;
}
