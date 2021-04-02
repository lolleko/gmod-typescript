import * as https from "https";
import * as querystring from "querystring";
import { WikiPage } from "../wiki_types";

const agent = new https.Agent({ maxSockets: 8 });

const baseRequestOptions: https.RequestOptions = {
    hostname: "wiki.facepunch.com",
    agent,
};

export async function GetPagesInCategory(category: string) {
    const bodyObj = {
        text: `<pagelist category="${category}"></pagelist>`,
        realm: "gmod",
    };

    const bodyStr = JSON.stringify(bodyObj);

    const options: https.RequestOptions = {
        ...baseRequestOptions,
        path: "/api/page/preview",
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Content-Length": bodyStr.length,
        },
    };

    return new Promise<string[]>((resolve, reject) => {
        let response = "";

        const req = https.request(options, (res) => {
            res.on("data", (chunk) => {
                response += chunk;
            });
        });

        req.on("close", () => {
            const responseObj: {
                status: string;
                html: string;
                title: string;
            } = JSON.parse(response);

            if (responseObj.status != "ok") {
                reject();
            }

            const refMatches = [...responseObj.html.matchAll(/href="(.*)"/g)];
            const refs = refMatches.map((rm) => rm[1]);

            resolve(refs);
        });

        req.on("error", (err) => {
            reject(err);
        });

        req.write(bodyStr);
        req.end();
    });
}

export async function GetPage(path: string) {
    const options: https.RequestOptions = {
        ...baseRequestOptions,
        path: `${path}?${querystring.stringify({ format: "json" })}`,
        method: "GET",
    };

    return new Promise<WikiPage>((resolve, reject) => {
        let response = "";

        const req = https.request(options, (res) => {
            res.on("data", (chunk) => {
                response += chunk;
            });
        });

        req.on("close", () => {
            resolve(JSON.parse(response) as WikiPage);
        });

        req.on("error", (err) => {
            reject(err);
        });

        req.end();
    });
}
