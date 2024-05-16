import { BaseParams } from "./BaseParams";

export class MemberParams extends BaseParams{
    gender: string;
    minAge = 18;
    maxAge = 99;
    orderBy = 'lastActive';

    constructor(gender: string) {
        super(); // Constructors for derived classes must contain a 'super' call.
        this.gender = gender === 'female' ? 'male' : 'female';
    }
}