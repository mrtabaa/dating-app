export class MemberParams {
    pageNumber = 1;
    pageSize = 10;
    gender: string;
    minAge = 18;
    maxAge = 99;
    orderBy = 'lastActive';

    constructor(gender: string) {
        this.gender = gender === 'female' ? 'male' : 'female';
    }
}