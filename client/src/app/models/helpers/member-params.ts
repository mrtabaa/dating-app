import {PaginationParams} from "./paginationParams";

export class MemberParams extends PaginationParams {
  userNameOrKnownAs: string | undefined;
  gender: string | undefined;
  minAge = 18;
  maxAge = 99;
  orderBy = 'lastActive';

  constructor(gender: string) {
    super(); // Constructors of derived classes must contain a 'super' call to properly set up the inheritance chain.
    this.gender = gender === 'female' ? 'male' : 'female';
  }
}
