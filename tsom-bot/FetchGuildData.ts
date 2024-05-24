export class FetchGuildData {

    public async fetchGuildData(): Promise<Response> {
        const url = "http://localhost:3000/guild";

        const body = {
            payload: {
                guildId: "l943tTO8QQ-_IwWHfwyJuQ",
                includeRecentGuildActivityInfo: true
            },
            enums: false
        }
        console.log(body);

        const requestHeaders: HeadersInit = new Headers();
        requestHeaders.set('content-type', 'application/json; charset=utf-8');
        requestHeaders.set('Origin', 'http://localhost:5500');

        let result;

        try {
            result = await fetch(url, {
                method: 'POST',
                headers: requestHeaders,
                body: JSON.stringify(body),
            })
        } catch (error) {
            console.log(error);
        }
        return await result.json();
    }
    constructor() {
        console.log();
    }
}