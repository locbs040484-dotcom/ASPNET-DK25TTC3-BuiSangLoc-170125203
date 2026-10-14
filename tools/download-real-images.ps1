$ErrorActionPreference = "Continue"
$root = Split-Path -Parent $PSScriptRoot
$src = Join-Path $root "wwwroot\images\_source"
$prod = Join-Path $root "wwwroot\images\products"
$banners = Join-Path $root "wwwroot\images\banners"
$cats = Join-Path $root "wwwroot\images\categories"
$blog = Join-Path $root "wwwroot\images\blog"
foreach ($d in @($src, $prod, $banners, $cats, $blog)) { New-Item -ItemType Directory -Force -Path $d | Out-Null }

function Get-Photo($url, $path) {
  if ((Test-Path $path) -and ((Get-Item $path).Length -gt 8000)) { return $true }
  try {
    curl.exe -fsSL --max-time 45 -A "Mozilla/5.0 LaptopZone" -o $path $url
    if ((Test-Path $path) -and ((Get-Item $path).Length -gt 8000)) { return $true }
  } catch {}
  return $false
}

# Real laptop / workspace photos (Unsplash + Pexels), cropped ~1000x750
$pool = @(
  "https://images.unsplash.com/photo-1603302576837-37561b2e82c4?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1625842268584-8f3296236761?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1593642702821-c8da6771f0c6?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1593642634315-48f5414c3ad9?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1593642634367-d91a135587b5?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1593642634443-44adaa737185?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1593642632559-0c6d3fc62b89?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1511385346081-47db9c5ca1db?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1525547719571-a2d4ac8945e2?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1611186871348-b1ce404a7196?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1484788984921-03950022c9ef?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1531297484001-80022131f5a1?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1498050108023-c5249f4df085?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1486312338219-ce68d2c6f44d?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1461749280684-dccba630e2f6?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1516387938699-a93567ec168e?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1515378791036-0648a3ef77b2?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1542751371-adc38448a05e?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1550745165-9bc0b252726f?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1600861194942-f883de0dfe96?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1641623410264-948701015656?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1675868374373-3a01b8da5ed6?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1515879218367-84677d680819?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?auto=format&fit=crop&w=1000&h=750&q=80",
  "https://images.pexels.com/photos/18105/pexels-photo.jpg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/7974/pexels-photo.jpg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/2047905/pexels-photo-2047905.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/1229861/pexels-photo-1229861.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/812264/pexels-photo-812264.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/129208/pexels-photo-129208.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/1029757/pexels-photo-1029757.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/461077/pexels-photo-461077.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/1181244/pexels-photo-1181244.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/1714208/pexels-photo-1714208.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/2528118/pexels-photo-2528118.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/3165335/pexels-photo-3165335.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/777001/pexels-photo-777001.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/1038916/pexels-photo-1038916.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/2399840/pexels-photo-2399840.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/1181675/pexels-photo-1181675.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/1181467/pexels-photo-1181467.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/1181298/pexels-photo-1181298.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/705264/pexels-photo-705264.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/374074/pexels-photo-374074.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/265667/pexels-photo-265667.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/5054213/pexels-photo-5054213.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/797582/pexels-photo-797582.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/2115256/pexels-photo-2115256.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/4009409/pexels-photo-4009409.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/2582937/pexels-photo-2582937.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/1714342/pexels-photo-1714342.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/326503/pexels-photo-326503.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/196655/pexels-photo-196655.jpeg?auto=compress&cs=tinysrgb&w=1200",
  "https://images.pexels.com/photos/669615/pexels-photo-669615.jpeg?auto=compress&cs=tinysrgb&w=1200"
)

$ok = @()
for ($i = 0; $i -lt $pool.Count; $i++) {
  $path = Join-Path $src ("pool-{0:D2}.jpg" -f $i)
  if (Get-Photo $pool[$i] $path) {
    $ok += $path
    Write-Host "OK pool-$i"
  } else {
    Write-Host "FAIL pool-$i"
  }
}

if ($ok.Count -lt 12) { throw "Downloaded too few photos: $($ok.Count)" }

$products = @(
  'asus-vivobook-15','asus-vivobook-14-oled','asus-zenbook-14-oled','asus-tuf-f15','asus-tuf-a15','asus-rog-strix-g16','asus-zephyrus-g14',
  'acer-aspire-5','acer-aspire-7','acer-swift-go-14','acer-nitro-v15','acer-nitro-16','acer-predator-neo-16',
  'dell-inspiron-15','dell-inspiron-14','dell-vostro-3530','dell-latitude-3540','dell-xps-13','dell-g15',
  'hp-15','hp-pavilion-15','hp-pavilion-plus-14','hp-victus-15','hp-victus-16','hp-envy-x360',
  'lenovo-ideapad-slim-3','lenovo-ideapad-slim-5','lenovo-thinkbook-14','lenovo-thinkpad-e14','lenovo-loq-15','lenovo-legion-5','lenovo-yoga-slim-7',
  'msi-modern-14','msi-modern-15','msi-cyborg-15','msi-katana-15','msi-thin-gf63',
  'macbook-air-m2-13','macbook-air-m3-13','macbook-air-m3-15','macbook-pro-m3-14'
)

# Unique primary per product; gallery uses next 4 distinct files from rotating pool
for ($p = 0; $p -lt $products.Count; $p++) {
  for ($k = 1; $k -le 5; $k++) {
    $srcFile = $ok[($p + ($k - 1) * 3) % $ok.Count]
    $dest = Join-Path $prod ("{0}-{1}.jpg" -f $products[$p], $k)
    Copy-Item $srcFile $dest -Force
  }
}
Copy-Item $ok[0] (Join-Path $prod "placeholder.jpg") -Force

# Banners + categories + blog from the most relevant files
Copy-Item $ok[0] (Join-Path $banners "banner-gaming.jpg") -Force
Copy-Item $ok[[Math]::Min(17, $ok.Count-1)] (Join-Path $banners "banner-office.jpg") -Force
Copy-Item $ok[[Math]::Min(8, $ok.Count-1)] (Join-Path $banners "banner-mac.jpg") -Force

$catMap = @{
  "gaming.jpg" = 0; "office.jpg" = 17; "student.jpg" = 16; "design.jpg" = 21
  "thin.jpg" = 9; "business.jpg" = 22; "mac.jpg" = 8; "workstation.jpg" = 18
}
foreach ($name in $catMap.Keys) {
  $idx = [Math]::Min([int]$catMap[$name], $ok.Count - 1)
  Copy-Item $ok[$idx] (Join-Path $cats $name) -Force
}

$blogs = @('blog-sv.jpg','blog-code.jpg','blog-ram.jpg','blog-cpu.jpg','blog-oled.jpg','blog-pin.jpg','blog-office.jpg','blog-tuf-loq.jpg','blog-m2m3.jpg','blog-check.jpg')
for ($i = 0; $i -lt $blogs.Count; $i++) {
  Copy-Item $ok[$i % $ok.Count] (Join-Path $blog $blogs[$i]) -Force
}

Write-Host "Downloaded $($ok.Count) unique photos and assigned product/banner/category images."
