# Plan: Tích hợp IAP + ShopScreen vào game Unity

Tài liệu này tổng hợp toàn bộ quy trình đã dùng để port bộ In-App Purchase từ game mẫu
`moneyrace-skybranch` sang `TapTapGame`, viết dưới dạng **playbook dùng lại được cho game khác**.

- **Game mẫu (nguồn):** `D:\DownGame\Skybranch\moneyrace-skybranch` — Unity 6000.3.9f1
- **Game đích (ví dụ đã làm):** `D:\DownGame\TapTapGame` — Unity 6000.5.7f1
- **Store:** Google Play Billing qua `com.unity.purchasing` 5.1.2

---

## 0. Thang giá & số coin

Giá thật **không nằm trong code** — nó được đặt trên Google Play Console. Code chỉ giữ:
product ID → số coin, và một chuỗi giá tạm để nhìn trong Editor.

| Product ID | Giá (Play Console) | Coin | Icon | coin/$ |
|---|---|---|---|---|
| `iap1` | 0,50 $ | 100 | `iv_gold1` | 200 |
| `iap2` | 1 $ | 200 | `iv_gold2` | 200 |
| `iap3` | 2 $ | 400 | `iv_gold3` | 200 |
| `iap4` | 3 $ | 600 | `iv_gold4` | 200 |
| `iap5` | 5 $ | 1000 | `iv_gold5` | 200 |
| `iap6` | 7 $ | 2000 | `iv_gold4` | 286 |
| `iap7` | 10 $ | 5000 | `iv_gold5` | 500 |

Chỉ có 5 ảnh `iv_gold1..5` cho 7 hàng nên hai hàng cuối dùng lại icon. (Game mẫu tệ hơn:
nó gán `iv_gold1` cho **cả 7** hàng, 4 ảnh còn lại không dùng.)

Bảng này xuất hiện ở **3 chỗ**, đổi giá/coin là phải sửa cả 3 cho khớp:

1. `Assets/Scripts/IAP/IAPManager.cs` → `GetCoinsForProduct()` + `[Header(...)]` + doc của `BuyCoinsNNN()`
2. `Assets/Scripts/IAP/ShopScreen.cs` → doc của `BuyCoinsNNN()`
3. `Assets/Editor/ShopScreenBuilder.cs` → mảng `Packs` (coin, icon, chuỗi giá tạm)

> **Loại sản phẩm:** mặc định `ProductType.NonConsumable` (giống game mẫu) ⇒ **mỗi gói chỉ mua
> được 1 lần/tài khoản**. Với gói coin, cái bạn gần như chắc chắn muốn là `Consumable`.
> Bật cờ `coinPacksAreConsumable` trên component IAPManager **và** đổi loại sản phẩm bên
> Play Console cho khớp. Đổi một bên thôi là hỏng.

---

## 1. Kiến trúc

```
                    ┌──────────────────┐
   Google Play ◄────┤   IAPManager     │  singleton, DontDestroyOnLoad
                    │  (IDetailedStore │  init trong Awake()
                    │    Listener)     │
                    └────────┬─────────┘
                             │ ProcessPurchase → CoinWallet.Add(n, flush:true)
                             ▼
                    ┌──────────────────┐
                    │   CoinWallet     │  static, PlayerPrefs, 1 key duy nhất
                    │  m_OnCoinsChanged│
                    └────┬─────────┬───┘
                         │         │
              ┌──────────▼──┐   ┌──▼─────────┐
              │  ShopScreen │   │  CoinHUD   │
              │ show/close  │   │ số coin HUD│
              └──────┬──────┘   └────────────┘
                     │ mỗi hàng gói:
                     ├─ Buy button  → UnityEvent → ShopScreen.BuyCoinsNNN() → IAPManager
                     └─ IAPButton   → IAPManager.BuyProduct(productId)
                                      + tự hiển thị localizedPriceString
```

**Nguyên tắc quan trọng:** chỉ có **một** nguồn sự thật cho số coin (`CoinWallet`).
Game mẫu vi phạm điều này và đó là bug thật của nó — xem mục 7.

---

## 2. Bộ file cần copy sang game mới

Copy nguyên các file này từ `TapTapGame`, rồi làm phần "cần sửa" ở cột phải.

### Runtime — `Assets/Scripts/`

| File | Vai trò | Cần sửa cho game mới |
|---|---|---|
| `IAP/IAPManager.cs` | Store listener, init, mua, grant coin, log | `namespace`; bảng coin trong `GetCoinsForProduct()` |
| `IAP/IAPButton.cs` | Nút mua + tự hiện giá nội tệ | `namespace` |
| `IAP/ShopScreen.cs` | Mở/đóng shop, nhãn coin, các `BuyCoinsNNN()` | `namespace`; bỏ `: Entity, IInstance` nếu game không có kiến trúc đó |
| `Services/CoinWallet.cs` | Ví coin bền vững (PlayerPrefs) | `namespace`; đổi `PPK_COINS` cho từng game |
| `UI/CoinHUD.cs` | Hiện số coin trên HUD | `namespace`; như trên |

### Editor — `Assets/Editor/`

| File | Vai trò | Cần sửa |
|---|---|---|
| `AddIAPManagerToScene.cs` | Tự thêm GameObject `IAPManager` khi vào Play + menu `Tools/…` | `GameplayScenePath` |
| `ShopScreenBuilder.cs` | Dựng cả màn Shop bằng code từ sprite | `CanvasName`, mảng `Packs`, toạ độ layout |

### Asset

| Đường dẫn | Nội dung |
|---|---|
| `Assets/Sprites/` | `bg_iap`, `header_iap`, `bg_button_iap`, `iv_buy`, `iv_back`, `iv_shop`, `iv_gold1..5`, `coin`, `cancel`, `remove-ads`, `rounded` |
| `Assets/Resources/BillingMode.json` | `{"androidStore":"GooglePlay"}` |
| `Assets/Resources/IAPProductCatalog.json` | catalog `iap1`–`iap7` |

> **Luôn copy kèm file `.meta`** của sprite. Nó mang GUID + `textureType: 8` (Sprite 2D and UI).
> Không có `.meta`, Unity import lại thành Texture thường và `LoadAssetAtPath<Sprite>` trả về null.

---

## 3. Cấu hình project

### 3.1 Package

`Packages/manifest.json`:
```json
"com.unity.purchasing": "5.1.2",
```
Kéo theo `com.unity.services.core`. Không cần cài tay gì thêm.

### 3.2 Project Settings

`ProjectSettings/ProjectSettings.asset`:
```yaml
useCustomMainManifest: 1
useCustomMainGradleTemplate: 1
cloudServicesEnabled:
  Purchasing: 1
AndroidReportGooglePlayAppDependencies: 1
```

`ProjectSettings/UnityConnectSettings.asset`:
```yaml
UnityPurchasingSettings:
  m_Enabled: 1
  m_TestMode: 0
```

### 3.3 Bắt buộc làm tay trong Player Settings

| Mục | Vì sao |
|---|---|
| **Package name** (Override Default Package Name) | Phải trùng app trên Play Console, nếu không store trả về 0 sản phẩm |
| **Scripting Backend = IL2CPP** | Play Store bắt buộc |
| **Target Architectures = ARMv7 + ARM64** | Play Store bắt buộc 64-bit |
| **Min SDK ≥ 21** | Play Billing 6/7 yêu cầu (26 là ổn) |
| **Keystore** | Bản upload phải ký |

### 3.4 `Assets/Plugins/Android/AndroidManifest.xml`

```xml
<uses-permission android:name="com.android.vending.BILLING" />
<uses-permission android:name="android.permission.INTERNET" />
```

Chỉ giữ **đúng một** block `<activity>` khớp *Application Entry Point*:

| `androidApplicationEntry` | Activity giữ lại |
|---|---|
| `1` (Activity) | `com.unity3d.player.UnityPlayerActivity` + theme `UnityThemeSelector` |
| `2` (GameActivity) | `com.unity3d.player.UnityPlayerGameActivity` + theme `BaseUnityGameActivityTheme` + `<meta-data android:name="android.app.lib_name" android:value="game"/>` |

Giữ cả hai ⇒ app có **2 icon launcher**. Game mẫu mắc lỗi này.

### 3.5 `Assets/Plugins/Android/mainTemplate.gradle`

```gradle
dependencies {
    implementation fileTree(dir: 'libs', include: ['*.jar'])
    implementation 'com.android.billingclient:billing:7.1.1'
**DEPS**}
```

> ⚠️ **Không copy `mainTemplate.gradle` giữa hai bản Unity khác nhau.** Template có token
> (`**DEPS**`, `**BUILDTOOLS**`, …) thay đổi theo phiên bản — ví dụ bản 6000.3 có
> `versionCode`/`versionName` và `buildToolsVersion = "…"` mà 6000.5 đã bỏ.
> Cách đúng: lấy template gốc của đúng bản Unity đang dùng tại
> `<Unity>/Editor/Data/PlaybackEngines/AndroidPlayer/Tools/GradleTemplates/mainTemplate.gradle`
> rồi chỉ chèn thêm dòng `billingclient`.

---

## 4. Dựng UI bằng ShopScreenBuilder

Màn shop của game mẫu nằm chết trong scene `Main.unity`, không có prefab ⇒ không copy file được.
Giải pháp: **generator chạy trong Editor**, menu `Tools/TapTap/Build Shop Screen`.

Cấu trúc nó sinh ra dưới `Canvas`:

```
Canvas
├── Coin HUD                      (Image coin + TMP)          → CoinHUD
├── Shop Button                   (Image iv_shop + Button)    → ShopScreen.ShowShopScreen
└── Shop                          (node LUÔN ACTIVE)          → ShopScreen
    └── Shop Panel  [inactive]    (Image bg_iap, full stretch)
        ├── Header                (thanh 1000x180, y -30)
        │   ├── Title             (TMP "SHOP", 68px)
        │   └── Back              (Image iv_back + Button)    → ShopScreen.CloseShopScreen
        ├── Coins Pill            (thanh 420x96, y -250)
        │   ├── Icon              (Image coin)
        │   └── Value             (TMP "{0}", vàng)
        └── Scroll View           (1000 rộng, y -400 .. đáy -50)
            └── Viewport          (RectMask2D)
                └── Content       (VerticalLayoutGroup + ContentSizeFitter)
                    └── Inapp1..7 (Image bg_button_iap, cao 200)
                        ├── Gold      (Image iv_goldN, 150x150)
                        ├── Title     (TMP "100 Coins", 50px)
                        ├── TextIap   (TMP + Button + IAPButton, vàng, canh phải)
                        └── Buy       (Image iv_buy 168x90 + Button) → ShopScreen.BuyCoinsNNN
```

### Số liệu bố cục (Canvas tham chiếu 1080×1920)

| Đại lượng | Giá trị |
|---|---|
| Bề rộng nội dung | 1080 − 2×40 = **1000** |
| Viewport scroll | 1920 − 400 − 50 = **1470** |
| Chiều cao content | 7×200 + 6×18 + 2×20 = **1548** |
| Kết quả | dư 78px ⇒ vẫn cuộn được, **không còn khoảng trống ở đáy** |
| Bề rộng hàng | 1000 − 2×20 = **960** |
| Cột trong hàng | `Gold 30..180 │ Title 210..550 │ Price 570..750 │ Buy 766..934` |

Chỉnh số gói mà vẫn muốn kín màn hình thì sửa `RowHeight` sao cho
`n×RowHeight + (n−1)×RowSpacing + 2×RowPadding` xấp xỉ **1470** (hơn một chút là đẹp nhất).

### Sáu chi tiết dễ sai khi tự viết generator

1. **Component `ShopScreen` phải nằm trên node luôn active**, không phải trên panel bị tắt.
   Đặt trên panel ⇒ `Awake()` không chạy khi panel inactive ⇒ không bao giờ mở được shop.
2. **Nối sự kiện bằng `UnityEventTools.AddVoidPersistentListener`**, không phải
   `onClick.AddListener`. Chỉ persistent listener mới được serialize vào scene.
3. **Panel nền phải bật `raycastTarget`** để chặn click xuyên xuống gameplay bên dưới.
4. **Bố cục hàng phải tính theo bề rộng thật**, đặt bằng anchor+pivot trái/phải. Đừng gọi
   `Place()` rồi ghi đè `pivot` sau — rect sẽ nhảy vị trí.
5. **Kiểm tra tỉ lệ ảnh trước khi ép kích thước.** `header_iap.png` là 1472×992 (tỉ lệ 1.48);
   ép thành thanh 1080×260 (tỉ lệ 4.15) làm banner bẹp dúm. Header trong bản này dùng
   `bg_button_iap` (1328×352, gần tỉ lệ thanh ngang) — đổi qua hằng `HeaderSprite`.
   Với icon (`iv_gold*`, `coin`, `iv_buy`) thì bật `preserveAspect`; với ảnh nền thì tắt.
6. **Ảnh header đã in sẵn chữ thì đừng thêm TMP tiêu đề** — sẽ chồng hai chữ "SHOP".
   Dùng hằng `ShowHeaderTitle` để bật/tắt. Lưu ý `header_iap.png` còn in cả tên game khác
   ("SKY BRANCH RUN"), sai thương hiệu nếu bê nguyên sang game mới.

---

## 5. Thứ tự thực hiện

1. Copy sprite + `.meta` → `Assets/Sprites/`
2. Copy `BillingMode.json`, `IAPProductCatalog.json` → `Assets/Resources/`
3. Copy scripts runtime + editor, sửa `namespace`
4. Thêm `com.unity.purchasing` vào `manifest.json`, mở Unity cho nó resolve
5. Sửa `ProjectSettings.asset` + `UnityConnectSettings.asset` (mục 3.2)
6. Tạo `AndroidManifest.xml` + `mainTemplate.gradle` (mục 3.4, 3.5)
7. Nối ví coin vào gameplay — trong `TapTapGame` là 1 dòng ở `GameLogic.OnCollectCoin()`:
   ```csharp
   m_Score++;
   CoinWallet.Add(1);   // Score reset mỗi ván, ví thì bền vững
   ```
8. `Tools/TapTap/Build Shop Screen`
9. `Tools/TapTap/Add IAPManager to Gameplay Scene`
10. Play test (mục 8)

---

## 6. Cấu hình Google Play Console

1. Tạo app, đặt package name **trùng** Player Settings.
2. Upload ít nhất 1 bản AAB đã ký lên **Internal testing** (bắt buộc — chưa có bản build thì
   sản phẩm IAP không active và store luôn trả về rỗng).
3. **Monetize → Products → In-app products**: tạo `iap1`…`iap7`, đặt giá theo bảng mục 0,
   chọn đúng loại (Consumable / One-time), rồi **Activate** từng cái.
4. **Setup → License testing**: thêm email tester để mua thử không mất tiền.
5. Tài khoản test phải join đường link Internal testing và cài app từ Play Store.

---

## 7. Những cái sai của game mẫu — đã sửa trong bản này

| Lỗi trong `moneyrace-skybranch` | Hậu quả | Bản này |
|---|---|---|
| `IAPManager.AddCoins()` ghi `PlayerPrefs["Coins"]`, còn HUD đọc `CoinManager_BR` với key `"SGLIB_COINS_BR"` | **Coin mua được không bao giờ hiện trong game** | Một nguồn duy nhất: `CoinWallet` |
| `IAPProductCatalog.json` khai `type: 0` (Consumable) nhưng code đăng ký `NonConsumable` | Hai nơi mâu thuẫn | Có cờ `coinPacksAreConsumable`, ghi rõ phải khớp Play Console |
| Hàng thứ 7 ghi title `"2000 Coins"` nhưng `iap7` cho 5000 | Sai lệch với người mua | Title sinh từ chính số coin |
| `Inapp5` bỏ trống `priceText` | Riêng hàng đó không hiện giá nội tệ | Generator gán đủ 7 hàng |
| `AddIAPManagerToScene` chỉ dò `GetComponent` trên root | Dễ tạo IAPManager trùng | Dùng `GetComponentInChildren<>(true)` |
| Manifest giữ cả `UnityPlayerActivity` và `UnityPlayerGameActivity` | 2 icon launcher | Giữ đúng 1, theo `androidApplicationEntry` |
| `PlayerPrefs.Save()` mỗi lần cộng coin | Khựng frame khi nhặt coin | `Add(amount, flush)` — chỉ flush khi giao dịch IAP |
| `iv_gold1` gán cho cả 7 hàng, 4 ảnh còn lại bỏ không | Các gói nhìn giống hệt nhau | Trải `iv_gold1..5` cho 7 hàng |
| Fake Store trả `localizedPriceString = "0"` và code ghi thẳng vào UI | Trong Editor cột giá hiện **"0"** thay vì giá | `IAPButton.HasRealPrice()` — không có chữ số khác 0 thì giữ chữ giá tạm |

Ngoài ra, Unity IAP 5.x đánh dấu API cũ (`UnityPurchasing.Initialize`, `IDetailedStoreListener`)
là `[Obsolete(..., false)]` — **cảnh báo, không phải lỗi**, vẫn chạy đầy đủ
(nằm ở `Runtime/Purchasing/Legacy/` trong package). `IAPManager.cs` có
`#pragma warning disable CS0618` để Console sạch.

---

## 8. Kiểm thử

### Trong Editor (Fake Store)
- Console có `[IAP] Initialized.` và bảng `LogFetchedProductValues()` liệt kê đủ 7 product.
- Nút `iv_shop` → panel mở, nút shop ẩn; `iv_back` → đóng lại.
- Bấm `Buy` → dialog Fake Store → Console `[IAP] Purchase OK: iapN`, HUD + nhãn trong shop
  tăng đúng số coin trong bảng.
- Thoát Play, Play lại → coin **giữ nguyên**.
- Nhặt coin trong ván → ví +1 và Score +1; chết & restart → Score về 0, ví giữ nguyên.

### Trên máy thật
- Build AAB (IL2CPP + ARM64, đã ký) → Internal testing → cài bằng tài khoản tester.
- Giá trên nút phải là **giá nội tệ thật** (`localizedPriceString`), không còn chuỗi tạm
  `$0.50`/`$1`/… Đây là bước duy nhất chứng minh product ID khớp Play Console —
  Fake Store trong Editor **không** kiểm được điều đó.
- Kiểm tra APK/AAB có `com.android.vending.BILLING` trong manifest đã merge.

### Bảng lỗi thường gặp

| Triệu chứng | Nguyên nhân hay gặp |
|---|---|
| `Init failed: NoProductsAvailable` | Product chưa Activate, hoặc chưa có bản build trên track nào |
| Store trả rỗng, không lỗi | Package name lệch với Play Console |
| Giá vẫn là chuỗi tạm | Chạy Fake Store, hoặc `priceText` chưa gán, hoặc chưa init xong |
| Mua lần 2 báo đã sở hữu | Product đang là `NonConsumable` — xem mục 0 |
| `Item not available for purchase` | Tài khoản chưa join Internal testing, hoặc cài app ngoài Play Store |
| Gradle lỗi token `**DEPS**` | `mainTemplate.gradle` lấy từ bản Unity khác — xem mục 3.5 |

---

## 9. Điều chỉnh cho game không có kiến trúc `Entity`/`Main`

`TapTapGame` có service locator riêng (`Main.Get<T>()`, `Entity`, `IInstance`). Nếu game mới
không có:

- `ShopScreen` và `CoinHUD`: đổi `: Entity, IInstance, IEnableEvent, IDisableEvent`
  → `: MonoBehaviour`, rồi đổi `OnEnableEvent()`/`OnDisableEvent()` thành `OnEnable()`/`OnDisable()`.
- Bỏ đoạn `SetEntityName(...)` trong `ShopScreenBuilder.cs`.
- `IAPManager`, `IAPButton`, `CoinWallet` **không phụ thuộc gì** vào kiến trúc đó — dùng nguyên.

Ngược lại, nếu game đích *có* kiến trúc kiểu Entity mà base class dùng `private void OnEnable()`,
tuyệt đối **không** khai báo `OnEnable()` ở lớp con — nó sẽ che hàm của base và làm hỏng
việc đăng ký sự kiện. Dùng interface như `IEnableEvent` mà base cung cấp.
