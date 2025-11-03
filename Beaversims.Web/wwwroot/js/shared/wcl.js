
import { cache, getOrSet } from "./cache.js";

const API = "https://www.warcraftlogs.com/api/v2/client";
const access_token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJhdWQiOiI5YjlkMWNkMC1iZDI1LTQ3YzctOWQ0OS1jODM3MmFlNWI1MGEiLCJqdGkiOiJjOWVmNWU2OWJmZDQ2MjhjY2RjZmMwMGQwODhiMThmMzRjOTU3MjM3YWU3ZDQyMjM1N2YyYTFlMTU5MjhkNzNhZTVhNjQxZDRlMzg4Yjc2MSIsImlhdCI6MTc1MzI2MDQyMi4xODcxNDYsIm5iZiI6MTc1MzI2MDQyMi4xODcxNSwiZXhwIjoxNzg0MzY0NDIyLjE3Njg3OCwic3ViIjoiIiwic2NvcGVzIjpbInZpZXctdXNlci1wcm9maWxlIiwidmlldy1wcml2YXRlLXJlcG9ydHMiXX0.AFnDJWwVIPMhgOxn7K-BxLdznk6rtDXUkOERAdCPBu2KFmJpJyVr5NokBKFIZOdIWolwX_3W6HEe-aNjrUj-2uRcWSQmcopu3QYBSbPCCWM_sBC74pVM2FqHY2LEw3Fwo5Xff6IHQ0VArX_ZYtSchXfhTuPDzQ9RQFYGYDyRGhhX98Vu8pvvmq1yLsST7P4uFGAskRSL2kN9vZ6H4w-sTMPJrjikJdj23d91SSwYOCkQG78rICRuWLCNuyRtDlaJLsSxi6eAZorNx9xgCSlzr90t6DoeF0xeWaBmEcrYERjqNTFCuEusaijQjFMvHyiupyxLZJqUjsLCWNUXLPRnCpS_rL9TjwRZ0ParRsC10_15UKtXIggE6-nQck9d2D-XSj174pbJsr2xvY5mkB8B2xYva-LteZz2bxTID7nmsNfE_dgkLvX8FsKBlSzh4K1iPzo1au6OQJqMs38Hvi7zmq0sd_mB90hpLl3GChhT2NrTVafnGbcMFshgIp_CidBDbZA5L4VO9KENdZhLjBI7W1JgtpBjyIPe2ALdEWE7DpzlGRq9aBfyM5NCpQOhavnJDOoWTdD6_48JJ_OYBKHwR_7i9-kcM8fkVfSqZ43c1ESrr-onEAohyaZaSNprUcmg06PLvC3zRXHcmfBWqmfcCHL5NYIGYVKm_6tzmgg-eqA";


export function getReportCodeFromUrl(url) {
    const m = String(url || "").match(/reports\/([A-Za-z0-9]{16})/);
    return m ? m[1] : null;
}

async function postGraphQL(query, variables) {
    const res = await fetch(API, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${access_token}`,
        },
        body: JSON.stringify({ query, variables }),
    });
    if (!res.ok) throw new Error(`WCL ${res.status}`);
    const json = await res.json();
    if (json.errors?.length) throw new Error(json.errors[0]?.message || "GraphQL error");
    return json;
}

export async function fetchFights(reportCode) {
    return getOrSet(cache.fights, reportCode, async () => {
        const query = `
      query($code:String!){
        reportData {
          report(code:$code) {
            fights { id name startTime endTime kill difficulty encounterID fightPercentage }
          }
        }
      }`;
        const { data } = await postGraphQL(query, { code: reportCode });
        return data?.reportData?.report?.fights ?? [];
    });
}

export async function fetchHealers(reportCode, fightId) {
    const key = `${reportCode}:${fightId}`;
    return getOrSet(cache.healers, key, async () => {
        const query = `
      query($code:String!,$fight:Int!){
        reportData {
          report(code:$code) {
            table(fightIDs:[$fight])
          }
        }
      }`;
        const { data } = await postGraphQL(query, { code: reportCode, fight: fightId });
        console.log(data);
        const healers = data?.reportData?.report?.table?.data?.playerDetails?.healers ?? [];
        return healers.map(h => ({
            id: h.id ?? h.guid,
            name: h.name,
            class: h.type,
            spec: (h.specs && h.specs[0]) || "",
            icon: h.icon,
            server: h.server,
            gear: h.combatantInfo.gear, 
            region: h.region,
        }));
    });
}

// prefer RAW TEXT to avoid double parse when passing to WASM
export async function fetchLogsRaw(reportCode, fightId, userId) {
    const key = `${reportCode}:${fightId}:${userId}`;
    return getOrSet(cache.logs, key, async () => {
        const query = `
        query {
            reportData {
                report(code: "${reportCode}") {
                    fightData: fights(
                        fightIDs: [${fightId}]
                    ) {
                        id,
                        startTime,
                        endTime,
                        encounterID,
                        fightPercentage,
                        inProgress,
                        name,
                        kill,
                        difficulty
                    }
                    userEvents: events(
                        fightIDs: [${fightId}],
                        useAbilityIDs: false,
                        includeResources: true,
                        sourceID: ${userId}
                    ) {
                        data
                    }
                    playerData: table(
                        fightIDs: [${fightId}]
                    )
                    combatantEvents: events(
                        dataType: CombatantInfo,
                        fightIDs: [${fightId}]
                    ) {
                        data
                    }
                }
            }
        }
    `;
        const res = await fetch(API, {
            method: "POST",
            headers: { "Content-Type": "application/json", Authorization: `Bearer ${access_token}` },
            body: JSON.stringify({ query, variables: { code: reportCode, fight: fightId, user: userId } }),
        });
        if (!res.ok) throw new Error(`WCL ${res.status}`);
        return await res.text(); // RAW JSON STRING
    });
}
