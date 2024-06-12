export interface UserUpdate {
    username?: string;
    knownAs: string;
    dateOfBirth?: Date,
    introduction: string;
    lookingFor: string;
    interests: string;
    countryAcr: string;
    country: string;
    state: string;
    city: string;
    isProfileCompoleted: boolean;
}