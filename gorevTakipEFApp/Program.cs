using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using GorevTakip.Data;

var builder = WebApplication.CreateBuilder(args);

// ── 1. MVC + "giriş zorunlu" filtresi ───────────────────────
//
// ⭐ Neden global filtre, her controller'a [Authorize] değil?
//    Yarın yeni bir controller yazıp [Authorize] koymayı unutursan
//    o sayfa herkese açık kalır. Global filtreyle varsayılan KAPALI olur.
//    GÜVENLİK İLKESİ: varsayılan hep en kısıtlayıcı seçenek olmalı.
builder.Services.AddControllersWithViews(secenekler =>
{
    var politika = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    secenekler.Filters.Add(new AuthorizeFilter(politika));
});

// ── 2. Çerez tabanlı kimlik doğrulama ───────────────────────
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(secenekler =>
    {
        secenekler.LoginPath = "/Hesap/Giris";
        secenekler.ReturnUrlParameter = "donusUrl";   // controller parametresiyle aynı olmalı!
        secenekler.ExpireTimeSpan = TimeSpan.FromHours(8);
        secenekler.SlidingExpiration = true;
        secenekler.Cookie.HttpOnly = true;             // JS erişemez → XSS koruması
        secenekler.Cookie.SameSite = SameSiteMode.Lax; // CSRF koruması
        secenekler.Cookie.Name = "GorevTakip.Oturum";
    });

// ⭐ DbContext'in "kim giriş yapmış?" sorusunu sorabilmesi için
builder.Services.AddHttpContextAccessor();

// ── 3. EF Core ──────────────────────────────────────────────
builder.Services.AddDbContext<GorevDbContext>(secenekler =>
{
    secenekler.UseSqlServer(
        builder.Configuration.GetConnectionString("GorevDb"));

    if (builder.Environment.IsDevelopment())
    {
        secenekler.LogTo(Console.WriteLine, LogLevel.Information);
        secenekler.EnableSensitiveDataLogging();   // ⚠️ canlıda ASLA
    }
});

var app = builder.Build();

// ── 4. Test verisi (sadece geliştirmede) ────────────────────
if (app.Environment.IsDevelopment())
{
    using (var kapsam = app.Services.CreateScope())
    {
        var db = kapsam.ServiceProvider.GetRequiredService<GorevDbContext>();
        await VeriTohumlayici.GorevleriEkleAsync(db);
    }
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ⭐⭐ SIRA KRİTİK
app.UseAuthentication();   // ÖNCE: "sen kimsin?" (çerezi okur)
app.UseAuthorization();    // SONRA: "girebilir mi?" (filtreyi uygular)

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();