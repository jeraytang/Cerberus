

create table if not exists Clients
(
    Id                                char(36)      not null
        primary key,
    Enabled                           bit           not null,
    ClientId                          varchar(100)  not null,
    ProtocolType                      varchar(25)  not null,
    ClientSecrets                     varchar(1500)   null,
    RequireClientSecret               bit           not null,
    ClientName                        varchar(100)  null,
    Description                       varchar(500) null,
    ClientUri                         varchar(1000) null,
    LogoUri                           varchar(1000) null,
    RequireConsent                    bit           not null,
    AllowRememberConsent              bit           not null,
    -- implicit hybrid authorization_code client_credentials password urn:ietf:params:oauth:grant-type:device_code 最长为 108
    AllowedGrantTypes                 varchar(120) null,
    AlwaysIncludeUserClaimsInIdToken  bit           not null,
    RequirePkce                       bit           not null,
    AllowPlainTextPkce                bit           not null,
    AllowAccessTokensViaBrowser       bit           not null,
    RedirectUris                      varchar(1500) null,
    PostLogoutRedirectUris            varchar(1500) null,
    FrontChannelLogoutUri             varchar(1000) null,
    FrontChannelLogoutSessionRequired bit           not null,
    BackChannelLogoutUri              varchar(1000) null,
    BackChannelLogoutSessionRequired  bit           not null,
    AllowOfflineAccess                bit           not null,
    AllowedScopes                     varchar(500) null,
    IdentityTokenLifetime             int           not null,
    AccessTokenLifetime               int           not null,
    AuthorizationCodeLifetime         int           not null,
    ConsentLifetime                   int           null,
    AbsoluteRefreshTokenLifetime      int           not null,
    SlidingRefreshTokenLifetime       int           not null,
    RefreshTokenUsage                 int           not null,
    UpdateAccessTokenClaimsOnRefresh  bit           not null,
    RefreshTokenExpiration            int           not null,
    AccessTokenType                   int           not null,
    EnableLocalLogin                  bit           not null,
    IdentityProviderRestrictions      varchar(1000) null,
    IncludeJwtId                      bit           not null,
    Claims                            varchar(500) null,
    AlwaysSendClientClaims            bit           not null,
    ClientClaimsPrefix                varchar(50)  null,
    PairWiseSubjectSalt               varchar(50)  null,
    UserSsoLifetime                   int           null,
    UserCodeType                      varchar(100)  null,
    DeviceCodeLifetime                int           not null,
    AllowedCorsOrigins                varchar(1500) null,
    Properties                        varchar(1000) null,
    -- audit columns
    CreationUserId                    varchar(40)   not null,
    CreationUserName                  varchar(255)  not null,
    CreationTime                      datetime(6)   not null,
    LastModificationUserId            varchar(40)   null,
    LastModificationUserName          varchar(255)  null,
    LastModificationTime              datetime(6)   null,
    constraint IX_Clients_ClientId
        unique (ClientId)
);

create table if not exists ClientCorsOrigins
(
    Id       char(36)
        primary key,
    Origin   varchar(150) not null,
    ClientId char(36)     not null,
    constraint FK_ClientCorsOrigins_Clients_ClientId
        foreign key (ClientId) references Clients (Id)
            on delete cascade,
    constraint IX_ClientCorsOrigins_ClientId_Origin
        unique (ClientId,Origin)
);

create table if not exists ApiResources
(
    Id           char(36)      not null
        primary key,
    Enabled      bit           not null,
    `Name`       varchar(200)  not null,
    DisplayName  varchar(200)  null,
    Description  varchar(500) null,
    UserClaims   varchar(500) null,
    Properties   varchar(1000) null,
    ApiSecrets   varchar(1500) null,
    -- audit columns
    CreationUserId                    varchar(40)   not null,
    CreationUserName                  varchar(255)  not null,
    CreationTime                      datetime(6)   not null,
    LastModificationUserId            varchar(40)   null,
    LastModificationUserName          varchar(255)  null,
    LastModificationTime              datetime(6)   null,
    constraint IX_ApiResources_Name
        unique (`Name`)
);

create table if not exists ApiScopes
(
    Id                      char(36)      not null
        primary key,
    `Name`                  varchar(200)  not null,
    DisplayName             varchar(200)  null,
    Description             varchar(500) null,
    Required                bit           not null,
    Emphasize               bit           not null,
    ShowInDiscoveryDocument bit           not null,
    UserClaims              varchar(500) null,
    ApiResourceId           char(36)   not null,
    -- audit columns
    CreationUserId                    varchar(40)   not null,
    CreationUserName                  varchar(255)  not null,
    CreationTime                      datetime(6)   not null,
    LastModificationUserId            varchar(40)   null,
    LastModificationUserName          varchar(255)  null,
    LastModificationTime              datetime(6)   null,
    constraint IX_ApiScopes_Name
        unique (`Name` ),
    constraint FK_ApiScopes_ApiResources_ApiResourceId
        foreign key (ApiResourceId) references ApiResources (Id)
            on delete cascade
);

create index IX_ApiScopes_ApiResourceId
    on ApiScopes (ApiResourceId);

create table if not exists IdentityResources
(
    Id                      char(36)      not null
        primary key,
    Enabled                 bit           not null,
    `Name`                  varchar(200)  not null,
    DisplayName             varchar(200)  null,
    Description             varchar(500) null,
    Required                bit           not null,
    Emphasize               bit           not null,
    ShowInDiscoveryDocument bit           not null,
    UserClaims              varchar(500) null,
    Properties              varchar(1000) null,
    -- audit columns
    CreationUserId                    varchar(40)   not null,
    CreationUserName                  varchar(255)  not null,
    CreationTime                      datetime(6)   not null,
    LastModificationUserId            varchar(40)   null,
    LastModificationUserName          varchar(255)  null,
    LastModificationTime              datetime(6)   null,
    constraint IX_IdentityResources_Name
        unique (`Name`)
);

create table if not exists PersistedGrants
(
    `Key`        varchar(200) not null
        primary key,
    `Type`       varchar(50)  not null,
    SubjectId    varchar(200) null,
    ClientId     varchar(200) not null,
    Expiration   datetime(6)  null,
    `Data`       longtext     not null,
    -- audit columns
    CreationUserId                    varchar(40)   not null,
    CreationUserName                  varchar(255)  not null,
    CreationTime                      datetime(6)   not null,
    LastModificationUserId            varchar(40)   null,
    LastModificationUserName          varchar(255)  null,
    LastModificationTime              datetime(6)   null
);

create index IX_PersistedGrants_SubjectId_ClientId_Type
    on PersistedGrants (SubjectId, ClientId, Type);

create table if not exists DeviceCodes
(
    UserCode     varchar(200) not null
        primary key,
    DeviceCode   varchar(200) not null,
    SubjectId    varchar(200) null,
    ClientId     varchar(200) not null,
    Expiration   datetime(6)  not null,
    Data         longtext     not null,
    CreationTime              datetime(6)   not null,
    constraint IX_DeviceCodes_DeviceCode
        unique (DeviceCode)
);

