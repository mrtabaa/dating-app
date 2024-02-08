import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable, map } from "rxjs";
import { PaginatedResult } from "../models/helpers/pagination";
import { inject } from "@angular/core";

export class PaginationHandler {
    private http = inject(HttpClient);

    /**
     * A reusable pagination method with generic type
     * @param url 
     * @param params 
     * @returns Observable<PaginatedResult<T>>
     */
    getPaginatedResult<T>(url: string, params: HttpParams): Observable<PaginatedResult<T>> {
        const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>;

        return this.http.get<T>(url, { observe: 'response', params }).pipe(
            map(response => {
                if (response.body)
                    paginatedResult.result = response.body // api's response body

                const pagination = response.headers.get('Pagination');
                if (pagination)
                    paginatedResult.pagination = JSON.parse(pagination); // api's response pagination values

                return paginatedResult;
            })
        );
    }
}