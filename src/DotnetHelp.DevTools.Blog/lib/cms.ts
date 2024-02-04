
export async function fetchCmsContent<T>(query: string, parameters?: { [key: string]: string | string[] }): Promise<T | null> {

    const response = await fetch(process.env.NEXT_HYGRAPH_API_URL!, {
        next: {
            revalidate: 60,
        },
        method: 'POST',
        headers: {
            'content-type': 'application/json'
        },
        body: JSON.stringify({
            query: query,
            variables: parameters
        }),
    });

    if (!response.ok) {
        const body = await response.json();
        throw new Error(`Invalid response code from CMS ${response.status}`, {
            cause: JSON.stringify(body, null, 2),
        });
    }

    return await response.json() as T;
}