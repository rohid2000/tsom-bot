export class GuildData {

    public async fetchGuildData(): Promise<Response> {
        const url = "https://localhost:3000/guild/";

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
            //mode: 'no-cors',   
            body: JSON.stringify(body),
            headers: {
                "content-type": "application/json;charset=UTF-8",
                "Access-Control-Allow-Origin": "*"
            }
        })
        return await result.json();
    }

    constructor() {
        console.log(this.fetchGuildData());
    }
}