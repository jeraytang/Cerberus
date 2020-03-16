# Cerberus

通用权限系统 + 认证 + 授权

## 组件

+ Cerberus 通用权限系统， 支持多租户。基于 Identity 开发
+ SecurityTokenService 认证授权系统的对外服务， 基于 IdentityServer4 开发
+ Admin 认证授权系统的管理平台

##  使用方法

## 启动 Cerberus API

1. 自动创建 Identity 相关表

2. 自动创建权限相关表

3. 添加默认管理员角色 admin, cerberus-admin

4. 添加默认管理员帐号 admin, 并赋于 admin、cerberus-admin 角色。此帐号的邮件地址和密码可通过配置文件 AdminEmail, AdminPassword 配置

```
  "Audience": "cerberus-api",
  "Authority": "http://localhost:7897",
  "RequireHttpsMetadata": false,
  "ApiSecret": "",
  "ConnectionString": "Database='Cerberus';Data Source=localhost;User ID=root;Password=1qazZAQ!;IgnoreCommandTransaction=true",
  "AdminEmail": "zlzforever@163.com",
  "AdminPassword": "1qazZAQ!",
```

### 启动 Cerberus

1. 修改配置文件

```
  "ClientId": "cerberus",
  "Authority": "http://localhost:7897",
  "RequireHttpsMetadata": false,
  "CerberusApi": "http://localhost:7895"
```

### 启动 SecurityTokenService

1. 此服务依赖 Identity 数据库， 若配置文件中 IdentityConnectionString 为空， 则表明使用自部署 Identity 模式。

2. 若使用自部署 Identity 服务， 则会自动创建管理角色 admin, 和 管理员帐户 admin， 可以通过配置文件 Admin：Email, Admin:Password 配置

3. 配置文件可以配置 IdentityServer4.Admin 的访问客户端， 若没有配置则会导致 IdentityServer4.Admin 无法通过 SecurityTokenService 的认证去管理 IdentityServer4

```
ClientId = identityserver4-admin,
ClientName = "IdentityServer4 admin",
AllowAccessTokensViaBrowser = false,
RequireClientSecret = true,
AllowedGrantTypes = new[] {GrantType.Implicit},
RedirectUris = new[] {"http://localhost:7898/signin-oidc"},
AllowedScopes = new[] {"openid", "profile"},
PostLogoutRedirectUris = new[] {"http://localhost:7898/signout-callback-oidc"},
ClientSecrets = new[] {new Secret("aabbccdd".ToSha256(), "default")},
AllowRememberConsent = true,
RequireConsent = false
```

### 启动 Admin

1. 修改配置文件

```
  "ClientId": "identityserver-admin",
  "Authority": "http://localhost:7897",
  "RequireHttpsMetadata": false,
  "Secret": "aabbccdd",
  "ConnectionString": "Database='IdentityServer4';Data Source=localhost;User ID=root;Password=1qazZAQ!;IgnoreCommandTransaction=true"
```

2. 启动后即可通过 SecurityTokenService 授权登录

3. 添加 IdentityResource： role

4. 添加 Cerberus 的 ApiResource

```
name: cerberus-api
user claimns: role name
```
4. 添加 Cerberus 客户端信息

```
ClientId: cerberus
AllowedGrantTypes: Implicit
Scopes: openid profile role cerberus-api
RedirectUris: http://localhost:7899/signin-oidc
PostLogoutRedirectUris: http://localhost:7899/signout-callback-oidc
RequireConsent: false
```

