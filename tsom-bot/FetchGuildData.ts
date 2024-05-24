export class FetchGuildData {

    public async fetchGuildData(): Promise<Response> {
        const url = "http://localhost:3000/guild";

        const body = {
            "payload": {
                "guildId": "l943tTO8QQ-_IwWHfwyJuQ",
                "includeRecentGuildActivityInfo": true
            },
            enums: false
        }
        console.log(body);

        let result;

        const headers = new Headers();
        headers.append("Content-Type", "application/json");

        try {
            result = await fetch(url, {
                method: 'POST',
                // headers: {
                // "content-type": "application/json; charset=utf-8",
                // "Origin": "http://localhost:5000"
                // },
                headers: headers,
                body: JSON.stringify(body),
            })
        } catch (error) {
            console.log(error);
        }
        console.log(result);
        return await result.json();
    }
    constructor() {
        console.log();
    }
}