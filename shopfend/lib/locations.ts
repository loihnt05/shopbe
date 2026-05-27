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
          name: "Quận Bình Thạnh",
          wards: ["Phường 1", "Phường 2", "Phường 3", "Phường 5", "Phường 6", "Phường 7"]
        },
        {
          name: "Quận 7",
          wards: ["Tân Kiểng", "Tân Phong", "Tân Phú", "Tân Quy", "Tân Thuận Đông", "Tân Thuận Tây"]
        }
      ]
    },
    {
      name: "Hà Nội",
      districts: [
        {
          name: "Quận Ba Đình",
          wards: ["Cống Vị", "Điện Biên", "Đội Cấn", "Giảng Võ", "Kim Mã", "Liễu Giai"]
        },
        {
          name: "Quận Hoàn Kiếm",
          wards: ["Chương Dương", "Cửa Đông", "Cửa Nam", "Đồng Xuân", "Hàng Bạc", "Hàng Bài"]
        }
      ]
    },
    {
      name: "Đà Nẵng",
      districts: [
        {
          name: "Quận Hải Châu",
          wards: ["Bình Hiên", "Bình Thuận", "Hòa Cường Bắc", "Hòa Cường Nam", "Hòa Thuận Đông"]
        }
      ]
    }
  ]
};
