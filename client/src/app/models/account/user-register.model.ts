import { NullableString } from "../../types/nullable-string.type";
import { OptionalString } from "../../types/optional-string.type";

export interface UserRegister{
    email: string;
    username: string;
    password: string;
    confirmPassword: string;
    dateOfBirth: string | undefined;
    knownAs: string;
    gender: string;
    city: string;
    country: string;
}