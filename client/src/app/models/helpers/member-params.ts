import { LoggedInUser } from "../logged-in-user.model";

export class MemberParams {
    pageNumber = 1;
    pageSize = 5;
    gender: string;
    minAge = 18;
    maxAge = 100;

    constructor(gender: string) {
        this.gender = gender === 'female' ? 'male' : 'female';
    }
}