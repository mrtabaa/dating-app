import { NullableString } from "../../types/nullable-string.type";
import { OptionalString } from "../../types/optional-string.type";

export interface UserRegister {
    email: NullableString;
    username: NullableString;
    password: NullableString;
    confirmPassword: NullableString;
    dateOfBirth: OptionalString,
    knownAs: string,
    gender: string,
    city: string,
    country: string
}