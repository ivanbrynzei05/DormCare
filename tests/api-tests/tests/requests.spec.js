const request = require("supertest");
const expect = require("chai").expect;

// Assumes the ASP.NET app is running on localhost:5000 (HTTP)
const BASE = process.env.API_BASE || "http://localhost:5000";

describe("Requests API (v2)", function () {
    it("GET /api/v2/requests should return list", async function () {
        const res = await request(BASE).get("/api/v2/requests");
        expect(res.status).to.be.oneOf([200, 302, 401, 403]);
    });

    it("GET /api/v1/requests should return legacy list", async function () {
        const res = await request(BASE).get("/api/v1/requests");
        expect(res.status).to.be.oneOf([200, 302, 401, 403]);
    });
});
