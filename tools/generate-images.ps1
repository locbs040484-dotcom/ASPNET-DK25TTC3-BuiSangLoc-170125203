$root = Split-Path -Parent $PSScriptRoot
$prod = Join-Path $root "wwwroot\images\products"
$brands = Join-Path $root "wwwroot\images\brands"
$banners = Join-Path $root "wwwroot\images\banners"
$blog = Join-Path $root "wwwroot\images\blog"
$cats = Join-Path $root "wwwroot\images\categories"
$avatars = Join-Path $root "wwwroot\images\avatars"
foreach ($d in @($prod,$brands,$banners,$blog,$cats,$avatars)) { New-Item -ItemType Directory -Force -Path $d | Out-Null }

function Write-Svg($path, $content) { Set-Content -Path $path -Value $content -Encoding UTF8 }

$brandColors = @{
  ASUS = @("#0F172A","#2563EB"); Acer = @("#1F2937","#16A34A"); Dell = @("#111827","#2563EB")
  HP = @("#0F172A","#1D4ED8"); Lenovo = @("#1E293B","#DC2626"); MSI = @("#111827","#DC2626")
  Apple = @("#111827","#6B7280"); Gigabyte = @("#0F172A","#F59E0B")
}

function LaptopSvg($title, $subtitle, $c1, $c2, $variant) {
  $screen = switch ($variant) {
    2 { $c2 }
    3 { "#111827" }
    4 { "#334155" }
    default { "#1E293B" }
  }
  $offset = 20 * ($variant - 1)
@"
<svg xmlns='http://www.w3.org/2000/svg' width='1000' height='750' viewBox='0 0 1000 750'>
  <rect width='1000' height='750' fill='#F3F4F6'/>
  <rect x='80' y='70' width='840' height='610' rx='28' fill='#FFFFFF' stroke='#E5E7EB'/>
  <rect x='$(190+$offset/4)' y='130' width='620' height='390' rx='18' fill='$c1'/>
  <rect x='$(210+$offset/4)' y='148' width='580' height='340' rx='8' fill='$screen'/>
  <text x='500' y='300' text-anchor='middle' fill='#FFFFFF' font-family='Inter,Arial' font-size='28' font-weight='700'>$title</text>
  <text x='500' y='340' text-anchor='middle' fill='#CBD5E1' font-family='Inter,Arial' font-size='16'>$subtitle</text>
  <rect x='160' y='530' width='680' height='28' rx='8' fill='#D1D5DB'/>
  <rect x='300' y='562' width='400' height='14' rx='6' fill='#9CA3AF'/>
  <circle cx='860' cy='160' r='8' fill='$c2'/>
</svg>
"@
}

$products = @(
  @{p='asus-vivobook-15';t='Vivobook 15';s='ASUS · Office';b='ASUS'},
  @{p='asus-vivobook-14-oled';t='Vivobook 14 OLED';s='ASUS · OLED';b='ASUS'},
  @{p='asus-zenbook-14-oled';t='Zenbook 14 OLED';s='ASUS · Premium';b='ASUS'},
  @{p='asus-tuf-f15';t='TUF Gaming F15';s='ASUS · RTX 4050';b='ASUS'},
  @{p='asus-tuf-a15';t='TUF Gaming A15';s='ASUS · RTX 4060';b='ASUS'},
  @{p='asus-rog-strix-g16';t='ROG Strix G16';s='ASUS · RTX 4070';b='ASUS'},
  @{p='asus-zephyrus-g14';t='Zephyrus G14';s='ASUS · Slim Gaming';b='ASUS'},
  @{p='acer-aspire-5';t='Aspire 5';s='Acer · Student';b='Acer'},
  @{p='acer-aspire-7';t='Aspire 7';s='Acer · RTX 2050';b='Acer'},
  @{p='acer-swift-go-14';t='Swift Go 14';s='Acer · OLED';b='Acer'},
  @{p='acer-nitro-v15';t='Nitro V 15';s='Acer · Gaming';b='Acer'},
  @{p='acer-nitro-16';t='Nitro 16';s='Acer · 165Hz';b='Acer'},
  @{p='acer-predator-neo-16';t='Predator Neo 16';s='Acer · RTX 4070';b='Acer'},
  @{p='dell-inspiron-15';t='Inspiron 15';s='Dell · Office';b='Dell'},
  @{p='dell-inspiron-14';t='Inspiron 14';s='Dell · 16:10';b='Dell'},
  @{p='dell-vostro-3530';t='Vostro 3530';s='Dell · SME';b='Dell'},
  @{p='dell-latitude-3540';t='Latitude 3540';s='Dell · Business';b='Dell'},
  @{p='dell-xps-13';t='XPS 13';s='Dell · Ultrabook';b='Dell'},
  @{p='dell-g15';t='G15 Gaming';s='Dell · RTX 4050';b='Dell'},
  @{p='hp-15';t='HP 15';s='HP · Everyday';b='HP'},
  @{p='hp-pavilion-15';t='Pavilion 15';s='HP · Office';b='HP'},
  @{p='hp-pavilion-plus-14';t='Pavilion Plus 14';s='HP · OLED';b='HP'},
  @{p='hp-victus-15';t='Victus 15';s='HP · Gaming';b='HP'},
  @{p='hp-victus-16';t='Victus 16';s='HP · RTX 4060';b='HP'},
  @{p='hp-envy-x360';t='Envy x360';s='HP · 2-in-1';b='HP'},
  @{p='lenovo-ideapad-slim-3';t='IdeaPad Slim 3';s='Lenovo · Student';b='Lenovo'},
  @{p='lenovo-ideapad-slim-5';t='IdeaPad Slim 5';s='Lenovo · OLED';b='Lenovo'},
  @{p='lenovo-thinkbook-14';t='ThinkBook 14';s='Lenovo · SME';b='Lenovo'},
  @{p='lenovo-thinkpad-e14';t='ThinkPad E14';s='Lenovo · Business';b='Lenovo'},
  @{p='lenovo-loq-15';t='LOQ 15';s='Lenovo · Gaming';b='Lenovo'},
  @{p='lenovo-legion-5';t='Legion 5';s='Lenovo · RTX 4060';b='Lenovo'},
  @{p='lenovo-yoga-slim-7';t='Yoga Slim 7';s='Lenovo · Premium';b='Lenovo'},
  @{p='msi-modern-14';t='Modern 14';s='MSI · Office';b='MSI'},
  @{p='msi-modern-15';t='Modern 15';s='MSI · Office';b='MSI'},
  @{p='msi-cyborg-15';t='Cyborg 15';s='MSI · RTX 4050';b='MSI'},
  @{p='msi-katana-15';t='Katana 15';s='MSI · RTX 4060';b='MSI'},
  @{p='msi-thin-gf63';t='Thin GF63';s='MSI · Slim Gaming';b='MSI'},
  @{p='macbook-air-m2-13';t='MacBook Air M2';s='Apple · 13 inch';b='Apple'},
  @{p='macbook-air-m3-13';t='MacBook Air M3';s='Apple · 13 inch';b='Apple'},
  @{p='macbook-air-m3-15';t='MacBook Air M3';s='Apple · 15 inch';b='Apple'},
  @{p='macbook-pro-m3-14';t='MacBook Pro M3';s='Apple · 14 inch';b='Apple'}
)

foreach ($item in $products) {
  $c = $brandColors[$item.b]
  1..5 | ForEach-Object {
    Write-Svg (Join-Path $prod "$($item.p)-$_.svg") (LaptopSvg $item.t "$($item.s) · góc $_" $c[0] $c[1] $_)
  }
}
Write-Svg (Join-Path $prod "placeholder.svg") (LaptopSvg "LaptopZone" "Product image" "#0F172A" "#2563EB" 1)

function BrandSvg($name, $c1) {
@"
<svg xmlns='http://www.w3.org/2000/svg' width='240' height='80' viewBox='0 0 240 80'>
  <rect width='240' height='80' fill='#FFFFFF'/>
  <text x='120' y='48' text-anchor='middle' font-family='Inter,Arial' font-size='28' font-weight='800' fill='$c1'>$name</text>
</svg>
"@
}
Write-Svg (Join-Path $brands "asus.svg") (BrandSvg "ASUS" "#0F172A")
Write-Svg (Join-Path $brands "acer.svg") (BrandSvg "Acer" "#16A34A")
Write-Svg (Join-Path $brands "dell.svg") (BrandSvg "DELL" "#2563EB")
Write-Svg (Join-Path $brands "hp.svg") (BrandSvg "HP" "#1D4ED8")
Write-Svg (Join-Path $brands "lenovo.svg") (BrandSvg "Lenovo" "#DC2626")
Write-Svg (Join-Path $brands "msi.svg") (BrandSvg "MSI" "#111827")
Write-Svg (Join-Path $brands "apple.svg") (BrandSvg "Apple" "#111827")
Write-Svg (Join-Path $brands "gigabyte.svg") (BrandSvg "GIGABYTE" "#F59E0B")

function BannerSvg($title, $sub, $c1, $c2) {
@"
<svg xmlns='http://www.w3.org/2000/svg' width='1200' height='750' viewBox='0 0 1200 750'>
  <rect width='1200' height='750' fill='$c1'/>
  <rect x='520' y='90' width='620' height='560' rx='28' fill='#0B1220' opacity='0.25'/>
  <rect x='580' y='160' width='500' height='320' rx='16' fill='#111827'/>
  <rect x='600' y='178' width='460' height='270' fill='$c2'/>
  <rect x='620' y='500' width='420' height='18' rx='6' fill='#94A3B8'/>
  <text x='80' y='280' fill='#FFFFFF' font-family='Inter,Arial' font-size='48' font-weight='800'>$title</text>
  <text x='80' y='340' fill='#E2E8F0' font-family='Inter,Arial' font-size='22'>$sub</text>
</svg>
"@
}
Write-Svg (Join-Path $banners "banner-gaming.svg") (BannerSvg "LAPTOP GAMING" "Hieu nang manh - gia tot moi ngay" "#0F172A" "#1D4ED8")
Write-Svg (Join-Path $banners "banner-office.svg") (BannerSvg "LAPTOP VAN PHONG" "Mong nhe - pin ben - lam viec ca ngay" "#111827" "#2563EB")
Write-Svg (Join-Path $banners "banner-mac.svg") (BannerSvg "MACBOOK" "Air va Pro chip M2 / M3" "#1E293B" "#64748B")
Write-Svg (Join-Path $banners "qr-demo.svg") @"
<svg xmlns='http://www.w3.org/2000/svg' width='220' height='220'><rect width='220' height='220' fill='#fff'/><rect x='20' y='20' width='40' height='40' fill='#111'/><rect x='160' y='20' width='40' height='40' fill='#111'/><rect x='20' y='160' width='40' height='40' fill='#111'/><rect x='80' y='80' width='60' height='60' fill='#111'/><text x='110' y='210' text-anchor='middle' font-size='12'>QR demo</text></svg>
"@

function BlogSvg($title, $c) {
@"
<svg xmlns='http://www.w3.org/2000/svg' width='800' height='500'><rect width='800' height='500' fill='$c'/><text x='40' y='260' fill='#fff' font-family='Inter,Arial' font-size='32' font-weight='700'>$title</text></svg>
"@
}
$blogs = @{
  'blog-sv.svg'='Laptop sinh vien'; 'blog-code.svg'='Hoc lap trinh'; 'blog-ram.svg'='RAM gaming'
  'blog-cpu.svg'='i5 vs Ryzen 5'; 'blog-oled.svg'='Man hinh OLED'; 'blog-pin.svg'='Bao quan pin'
  'blog-office.svg'='Laptop van phong'; 'blog-tuf-loq.svg'='TUF vs LOQ'; 'blog-m2m3.svg'='M2 hay M3'
  'blog-check.svg'='Check may moi'
}
$i=0; $cols=@('#0F172A','#1D4ED8','#111827','#334155','#1E3A8A','#0F766E','#7C2D12','#1E293B','#334155','#0F172A')
foreach ($k in $blogs.Keys) { Write-Svg (Join-Path $blog $k) (BlogSvg $blogs[$k] $cols[$i]); $i++ }

function CatSvg($label,$c){"<svg xmlns='http://www.w3.org/2000/svg' width='160' height='160'><rect width='160' height='160' rx='16' fill='$c'/><text x='80' y='88' text-anchor='middle' fill='#fff' font-size='16' font-family='Inter,Arial'>$label</text></svg>"}
Write-Svg (Join-Path $cats "gaming.svg") (CatSvg "Gaming" "#1D4ED8")
Write-Svg (Join-Path $cats "office.svg") (CatSvg "Office" "#0F172A")
Write-Svg (Join-Path $cats "student.svg") (CatSvg "Student" "#2563EB")
Write-Svg (Join-Path $cats "design.svg") (CatSvg "Design" "#7C3AED")
Write-Svg (Join-Path $cats "thin.svg") (CatSvg "Thin" "#0EA5E9")
Write-Svg (Join-Path $cats "business.svg") (CatSvg "Business" "#111827")
Write-Svg (Join-Path $cats "mac.svg") (CatSvg "MacBook" "#334155")
Write-Svg (Join-Path $cats "workstation.svg") (CatSvg "Workstation" "#1E293B")

Write-Svg (Join-Path $avatars "default.svg") "<svg xmlns='http://www.w3.org/2000/svg' width='160' height='160'><circle cx='80' cy='80' r='80' fill='#E5E7EB'/><circle cx='80' cy='66' r='28' fill='#94A3B8'/><ellipse cx='80' cy='140' rx='46' ry='30' fill='#94A3B8'/></svg>"
Write-Host "Images generated"
