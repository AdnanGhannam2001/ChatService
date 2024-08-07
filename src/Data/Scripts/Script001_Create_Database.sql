CREATE TABLE IF NOT EXISTS "Chats" (
    "Id" VARCHAR(255) NOT NULL,
    "IsGroup" BOOLEAN DEFAULT FALSE,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "LastMessageAt" TIMESTAMPTZ,
    PRIMARY KEY ("Id"));

CREATE TABLE IF NOT EXISTS "Members" (
    "ChatId" VARCHAR(255) NOT NULL,
    "UserId" VARCHAR(255) NOT NULL,
    "Role" INTEGER NOT NULL,
    PRIMARY KEY ("ChatId", "UserId"),
    FOREIGN KEY ("ChatId") REFERENCES "Chats"("Id"));

CREATE TABLE IF NOT EXISTS "Messages" (
    "Id" VARCHAR(255) NOT NULL,
    "SenderId" VARCHAR(255) NOT NULL,
    "ChatId" VARCHAR(255) NOT NULL,
    "Content" VARCHAR(1000) NOT NULL, 
    "SentAt" TIMESTAMPTZ,
    "LastUpdateAt" TIMESTAMPTZ,
    PRIMARY KEY ("Id"),
    FOREIGN KEY ("ChatId") REFERENCES "Chats"("Id"));