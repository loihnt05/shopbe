import { describe, expect, it } from "vitest";
import { productResponseToListItem } from "../shopbeApi";

describe("productResponseToListItem", () => {
  it("maps PascalCase discover payload image fields", () => {
    const product = productResponseToListItem({
      Id: "product-1",
      Name: "Discover Product",
      Slug: "discover-product",
      PrimaryImageUrl: "https://example.com/primary.jpg",
      Images: [
        {
          Id: "image-1",
          ImageUrl: "https://example.com/primary.jpg",
          IsPrimary: true,
        },
      ],
      CategoryId: "category-1",
      IsActive: true,
    });

    expect(product.primaryImageUrl).toBe("https://example.com/primary.jpg");
    expect(product.images?.[0]).toMatchObject({
      id: "image-1",
      imageUrl: "https://example.com/primary.jpg",
      isPrimary: true,
    });
  });

  it("falls back to images when primaryImageUrl is blank", () => {
    const product = productResponseToListItem({
      id: "product-1",
      name: "Fallback Product",
      slug: "fallback-product",
      primaryImageUrl: "",
      images: [
        {
          id: "image-1",
          imageUrl: "https://example.com/fallback.jpg",
          isPrimary: true,
        },
      ],
      categoryId: "category-1",
      isActive: true,
    });

    expect(product.primaryImageUrl).toBe("https://example.com/fallback.jpg");
  });
});
