# OdemeTopla

  Çevirmenlik yapanlar için SmartCat ve Beluga sitelerindeki bekleyen ödemeleri toplayan program.
  Bu projede Selenium kütüphanesi kullanılmıştır. Kur verileri xml ile güncel çekilmektedir.
  SmartCat sitesinde hesap dışında kalması istenilen ödemeler için projenin publish dosyasının ana dizinine excludedList adında json dosyası listesi içine job id'ler yazılarak yapılabilir.
  
  ## Örnek excludedList düzeni
  
```
[
  "KKTC BANKACILIK YASASI_раб",
  "hesap-basvuru-formu-tr_раб",
  "Help Center - Translation (Beluga)"
]
```
