export class FetchGuildData {

    public async fetchGuildData(): Promise<Response> {
        const url = "http://172.26.0.1:3000/guild";

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
        headers.append("Access-Control-Allow-Origin", "*");
        headers.append("Access-Control-Allow-Methods", "DELETE, POST, GET, OPTIONS");
        headers.append("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");

        try {
            result = await fetch(url, {
                method: 'POST',
                headers: headers,
                // headers: {
                // "content-type": "application/json; charset=utf-8",
                // "Origin": "http://localhost:5000"
                // },
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