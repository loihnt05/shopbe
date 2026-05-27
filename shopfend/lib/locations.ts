export type LocationData = {
  provinces: {
    name: string;
    districts: {
      name: string;
      wards: string[];
    }[];
  }[];
};

export const locations: LocationData = {
  provinces: [
    {
      name: "Hồ Chí Minh",
      districts: [
        {
          name: "Quận 1",
          wards: ["Bến Nghé", "Bến Thành", "Cô Giang", "Đa Kao", "Nguyễn Cư Trinh", "Nguyễn Thái Bình", "Phạm Ngũ Lão", "Phường Tân Định"]
        },
        {
          name: "Quận 3",
          wards: ["Võ Thị Sáu", "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5"]
        },
        {
          name: "Quận 4",
          wards: ["Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 6", "Phường 8", "Phường 9", "Phường 10", "Phường 13", "Phường 14", "Phường 15", "Phường 16", "Phường 18"]
        },
        {
          name: "Quận 5",
          wards: ["Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14"]
        },
        {
          name: "Quận 10",
          wards: ["Phường 1", "Phường 2", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15"]
        },
        {
          name: "Quận 11",
          wards: ["Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 16"]
        },
        {
          name: "Quận Phú Nhuận",
          wards: ["Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 13", "Phường 15", "Phường 17"]
        },
        {
          name: "Quận Bình Thạnh",
          wards: ["Phường 1", "Phường 2", "Phường 3", "Phường 5", "Phường 6", "Phường 7", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 17", "Phường 19", "Phường 21", "Phường 22", "Phường 24", "Phường 25", "Phường 26", "Phường 27", "Phường 28"]
        },
        {
          name: "Quận Tân Bình",
          wards: ["Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15"]
        },
        {
          name: "Quận Gò Vấp",
          wards: ["Phường 1", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 16", "Phường 17"]
        },
        {
          name: "Quận 7",
          wards: ["Bình Thuận", "Phú Mỹ", "Phú Thuận", "Tân Hưng", "Tân Kiểng", "Tân Phong", "Tân Phú", "Tân Quy", "Tân Thuận Đông", "Tân Thuận Tây"]
        },
        {
          name: "Quận 8",
          wards: ["Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 16"]
        },
        {
          name: "Quận 12",
          wards: ["An Phú Đông", "Đông Hưng Thuận", "Hiệp Thành", "Tân Chánh Hiệp", "Tân Hưng Thuận", "Tân Thới Hiệp", "Tân Thới Nhất", "Thạnh Lộc", "Thạnh Xuân", "Thới An", "Trung Mỹ Tây"]
        },
        {
          name: "Quận Bình Tân",
          wards: ["An Lạc", "An Lạc A", "Bình Hưng Hòa", "Bình Hưng Hòa A", "Bình Hưng Hòa B", "Bình Trị Đông", "Bình Trị Đông A", "Bình Trị Đông B", "Tân Tạo", "Tân Tạo A"]
        },
        {
          name: "Thành phố Thủ Đức",
          wards: ["An Khánh", "An Lợi Đông", "An Phú", "Bình Chiểu", "Bình Thọ", "Cát Lái", "Hiệp Bình Chánh", "Hiệp Bình Phước", "Hiệp Phú", "Linh Chiểu", "Linh Đông", "Linh Tây", "Linh Trung", "Linh Xuân", "Long Bình", "Long Phước", "Long Thạnh Mỹ", "Long Trường", "Phú Hữu", "Phước Bình", "Phước Long A", "Phước Long B", "Tam Bình", "Tam Phú", "Tăng Nhơn Phú A", "Tăng Nhơn Phú B", "Thạnh Mỹ Lợi", "Thảo Điền", "Thủ Thiêm", "Trường Thạnh", "Trường Thọ"]
        },
        {
          name: "Huyện Nhà Bè",
          wards: ["Thị trấn Nhà Bè", "Hiệp Phước", "Long Thới", "Nhơn Đức", "Phú Xuân", "Phước Kiển", "Phước Lộc"]
        },
        {
          name: "Huyện Hóc Môn",
          wards: ["Thị trấn Hóc Môn", "Bà Điểm", "Đông Thạnh", "Nhị Bình", "Tân Hiệp", "Tân Thới Nhì", "Tân Xuân", "Thới Tam Thôn", "Trung Chánh", "Xuân Thới Đông", "Xuân Thới Sơn", "Xuân Thới Thượng"]
        },
        {
          name: "Huyện Bình Chánh",
          wards: ["Thị trấn Tân Túc", "An Phú Tây", "Bình Chánh", "Bình Hưng", "Bình Lợi", "Đa Phước", "Hưng Long", "Lê Minh Xuân", "Phạm Văn Hai", "Phong Phú", "Quy Đức", "Tân Kiên", "Tân Nhựt", "Tân Quý Tây", "Vĩnh Lộc A", "Vĩnh Lộc B"]
        },
        {
          name: "Huyện Củ Chi",
          wards: ["Thị trấn Củ Chi", "An Nhơn Tây", "An Phú", "Bình Mỹ", "Hòa Phú", "Nhuận Đức", "Phạm Văn Cội", "Phú Hòa Đông", "Phú Mỹ Hưng", "Phước Hiệp", "Phước Thạnh", "Phước Vĩnh An", "Tân An Hội", "Tân Phú Trung", "Tân Thạnh Đông", "Tân Thạnh Tây", "Tân Thông Hội", "Thái Mỹ", "Trung An", "Trung Lập Hạ", "Trung Lập Thượng"]
        },
        {
          name: "Huyện Cần Giờ",
          wards: ["Thị trấn Cần Thạnh", "An Thới Đông", "Bình Khánh", "Long Hòa", "Lý Nhơn", "Tam Thôn Hiệp", "Thạnh An"]
        }
      ]
    },
    {
      name: "Hà Nội",
      districts: [
        {
          name: "Quận Ba Đình",
          wards: ["Cống Vị", "Điện Biên", "Đội Cấn", "Giảng Võ", "Kim Mã", "Liễu Giai", "Ngọc Hà", "Ngọc Khánh", "Nguyễn Trung Trực", "Phúc Xá", "Quán Thánh", "Thành Công", "Trúc Bạch", "Vĩnh Phúc"]
        },
        {
          name: "Quận Hoàn Kiếm",
          wards: ["Chương Dương", "Cửa Đông", "Cửa Nam", "Đồng Xuân", "Hàng Bạc", "Hàng Bài", "Hàng Bồ", "Hàng Bông", "Hàng Buồm", "Hàng Đào", "Hàng Gai", "Hàng Mã", "Hàng Trống", "Lý Thái Tổ", "Phan Chu Trinh", "Phúc Tân", "Trần Hưng Đạo", "Tràng Tiền"]
        },
        {
          name: "Quận Tây Hồ",
          wards: ["Bưởi", "Nhật Tân", "Phú Thượng", "Quảng An", "Thụy Khuê", "Tứ Liên", "Xuân La", "Yên Phụ"]
        },
        {
          name: "Quận Cầu Giấy",
          wards: ["Dịch Vọng", "Dịch Vọng Hậu", "Mai Dịch", "Nghĩa Đô", "Nghĩa Tân", "Quan Hoa", "Trung Hòa", "Yên Hòa"]
        },
        {
          name: "Quận Đống Đa",
          wards: ["Cát Linh", "Hàng Bột", "Khâm Thiên", "Khương Thượng", "Kim Liên", "Láng Hạ", "Láng Thượng", "Nam Đồng", "Ngã Tư Sở", "Ô Chợ Dừa", "Phương Liên", "Phương Mai", "Quang Trung", "Quốc Tử Giám", "Thịnh Quang", "Thổ Quan", "Trung Phụng", "Trung Liệt", "Trung Tự", "Văn Chương", "Văn Miếu"]
        },
        {
          name: "Quận Hai Bà Trưng",
          wards: ["Bách Khoa", "Bạch Đằng", "Bạch Mai", "Cầu Dền", "Đống Mác", "Đồng Nhân", "Đồng Tâm", "Lê Đại Hành", "Minh Khai", "Nguyễn Du", "Phạm Đình Hổ", "Phố Huế", "Quỳnh Lôi", "Quỳnh Mai", "Thanh Lương", "Thanh Nhàn", "Trương Định", "Vĩnh Tuy"]
        }
      ]
    },
    {
      name: "Đà Nẵng",
      districts: [
        {
          name: "Quận Hải Châu",
          wards: ["Bình Hiên", "Bình Thuận", "Hòa Cường Bắc", "Hòa Cường Nam", "Hòa Thuận Đông", "Hòa Thuận Tây", "Nam Dương", "Phước Ninh", "Thạch Thang", "Thanh Bình", "Thuận Phước", "Hải Châu I", "Hải Châu II"]
        },
        {
          name: "Quận Thanh Khê",
          wards: ["An Khê", "Chính Gián", "Hòa Khê", "Phước Ninh", "Tam Thuận", "Thạc Gián", "Thanh Khê Đông", "Thanh Khê Tây", "Vĩnh Trung", "Xuân Hà"]
        },
        {
          name: "Quận Sơn Trà",
          wards: ["An Hải Bắc", "An Hải Đông", "An Hải Tây", "Mân Thái", "Nại Hiên Đông", "Phước Mỹ", "Thọ Quang"]
        }
      ]
    },
    {
      name: "Cần Thơ",
      districts: [
        {
          name: "Quận Ninh Kiều",
          wards: ["An Bình", "An Cư", "An Hòa", "An Khánh", "An Lạc", "An Nghiệp", "An Phú", "An Thới", "Cái Khế", "Hưng Lợi", "Tân An", "Thới Bình", "Xuân Khánh"]
        },
        {
          name: "Quận Bình Thủy",
          wards: ["An Thới", "Bình Thủy", "Bùi Hữu Nghĩa", "Long Hòa", "Long Tuyền", "Thới An Đông", "Trà An", "Trà Nóc"]
        }
      ]
    },
    {
      name: "Hải Phòng",
      districts: [
        {
          name: "Quận Hồng Bàng",
          wards: ["Hạ Lý", "Hoàng Văn Thụ", "Minh Khai", "Phạm Hồng Thái", "Phan Bội Châu", "Quán Toan", "Sở Dầu", "Thượng Lý", "Trại Chuối"]
        },
        {
          name: "Quận Ngô Quyền",
          wards: ["Cầu Đất", "Cầu Tre", "Đằng Giang", "Gia Viên", "Lạc Viên", "Lạch Tray", "Lê Lợi", "Máy Chai", "Máy Tơ", "Vạn Mỹ"]
        }
      ]
    },
    {
      name: "Lâm Đồng",
      districts: [
        {
          name: "Thành phố Đà Lạt",
          wards: ["Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Tà Nung", "Trạm Hành", "Xuân Thọ", "Xuân Trường"]
        },
        {
          name: "Huyện Lạc Dương",
          wards: ["Thị trấn Lạc Dương", "Đạ Chais", "Đạ Nhim", "Đạ Sar", "Đưng Knớ", "Lát"]
        }
      ]
    },
    {
      name: "Hà Giang",
      districts: [
        {
          name: "Huyện Đồng Văn",
          wards: ["Thị trấn Đồng Văn", "Thị trấn Phó Bảng", "Hố Quáng Phìn", "Lũng Cú", "Lũng Phìn", "Lũng Táo", "Lũng Thầu", "Má Lé", "Phố Cáo", "Phố Là", "Sà Phìn", "Sảng Tủng", "Sính Lủng", "Sủng Là", "Sủng Trái", "Tả Phìn", "Tả Lũng", "Thải Phìn Tủng", "Vần Chải"]
        },
        {
          name: "Huyện Mèo Vạc",
          wards: ["Thị trấn Mèo Vạc", "Cán Chu Phìn", "Giàng Chu Phìn", "Khâu Vai", "Lũng Chinh", "Lũng Pù", "Nậm Ban", "Niêm Sơn", "Niêm Tòng", "Pả Vi", "Pải Lủng", "Sơn Vĩ", "Sủng Máng", "Sủng Trà", "Tả Lủng", "Tát Ngà", "Thượng Phùng", "Xín Cái"]
        }
      ]
    },
    {
      name: "Lào Cai",
      districts: [
        {
          name: "Thành phố Sa Pa",
          wards: ["Phường Cầu Mây", "Phường Hàm Rồng", "Phường Ô Quý Hồ", "Phường Phan Si Păng", "Phường Sa Pa", "Phường Sa Pả", "Xã Bản Hồ", "Xã Hoàng Liên", "Xã Liên Minh", "Xã Mường Bo", "Xã Mường Hoa", "Xã Ngũ Chỉ Sơn", "Xã Tả Phìn", "Xã Tả Van", "Xã Thanh Bình", "Xã Trung Chải"]
        }
      ]
    },
    {
      name: "Cao Bằng",
      districts: [
        {
          name: "Huyện Trùng Khánh",
          wards: ["Thị trấn Trùng Khánh", "Thị trấn Trà Lĩnh", "Cao Chương", "Cảnh Tiên", "Chí Viễn", "Đàm Thủy", "Đình Phong", "Đoài Dương", "Đức Hồng", "Khâm Thành", "Lăng Hiếu", "Ngọc Côn", "Ngọc Đào", "Ngọc Khê", "Phong Châu", "Phong Nậm", "Quang Hán", "Quang Trung", "Quang Vinh", "Tri Phương", "Trung Phúc"]
        }
      ]
    },
    {
      name: "Kiên Giang",
      districts: [
        {
          name: "Thành phố Phú Quốc",
          wards: ["Phường Dương Đông", "Phường An Thới", "Xã Bãi Thơm", "Xã Cửa Cạn", "Xã Cửa Dương", "Xã Dương Tơ", "Xã Gành Dầu", "Xã Hàm Ninh", "Xã Thổ Châu"]
        }
      ]
    },
    {
      name: "Bình Thuận",
      districts: [
        {
          name: "Huyện Đảo Phú Quý",
          wards: ["Tam Thanh", "Ngũ Phụng", "Long Hải"]
        }
      ]
    },
    {
      name: "Quảng Nam",
      districts: [
        {
          name: "Huyện Nam Trà My",
          wards: ["Trà Mai", "Trà Can", "Trà Don", "Trà Dơn", "Trà Leng", "Trà Linh", "Trà Nam", "Trà Tập", "Trà Vân", "Trà Vinh"]
        }
      ]
    },
    {
      name: "Kon Tum",
      districts: [
        {
          name: "Huyện Tu Mơ Rông",
          wards: ["Đăk Hà", "Đăk Na", "Đăk Rơ Ông", "Đăk Sao", "Đăk Tờ Kan", "Măng Ri", "Ngọk Lây", "Ngọk Yêu", "Tê Xăng", "Tu Mơ Rông", "Văn Xuôi"]
        }
      ]
    },
    {
      name: "Đắk Lắk",
      districts: [
        {
          name: "Huyện Lắk",
          wards: ["Thị trấn Liên Sơn", "Bông Krang", "Đắk Liêng", "Đắk Nuê", "Đắk Phơi", "Krông Nô", "Nam Ka", "Phi Liêng", "Yang Tao"]
        }
      ]
    }
  ]
};
