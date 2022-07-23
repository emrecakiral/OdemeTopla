# SmartCat ve Beluga Alacak Hesabı

  Çevirmenlik yapanlar için SmartCat ve Beluga sitelerindeki bekleyen ödemeleri toplayan program.
  Bu projede Selenium kütüphanesi kullanılmıştır. Kur verileri XML ile güncel çekilmektedir.
  SmartCat sitesinde hesap dışında kalması istenilen ödemeler için projenin publish dosyasının ana dizinine excludedList adında json dosyası içine liste olarak job id'ler yazılarak yapılabilir.
  
  ## Örnek excludedList düzeni
  
```
[
  "KKTC BANKACILIK YASASI_раб",
  "hesap-basvuru-formu-tr_раб",
  "Help Center - Translation (Beluga)"
]
```
