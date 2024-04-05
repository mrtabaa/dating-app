import { NullableString } from "../types/nullable-string.type";

export interface Country {
    code: NullableString,
    acr: NullableString,
    name: NullableString,
    shortName: NullableString,
}