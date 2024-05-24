import { FetchGuildData } from "./FetchGuildData.js";

const guildData = new FetchGuildData();

console.log(guildData.fetchGuildData());

const fetchData = document.getElementById("fetchButton");

fetchData.addEventListener("click", async () => {
    console.log(await guildData.fetchGuildData());
});