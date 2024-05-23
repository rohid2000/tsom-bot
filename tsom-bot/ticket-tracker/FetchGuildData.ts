export class FetchGuildData {

    public async fetchGuildData(): Promise<Response> {
        const url = "http:// 172.25.144.1:3000/guild";

        const body = {
            payload: {
                guildId: "l943tTO8QQ-_IwWHfwyJuQ",
                includeRecentGuildActivityInfo: true
            },
            enums: false
        }
        console.log(body);

        const result = await fetch(url, {
            method: "POST",
            //body: JSON.stringify(body),
            headers: {
                "Content-Type": "application/json;charset=UTF-8",
                "Access-Control-Allow-Origin": "*"
            },
            body: JSON.stringify(body)
        })
        return await result.json();
    }

    constructor() {
        console.log(this.fetchGuildData());
    }
}