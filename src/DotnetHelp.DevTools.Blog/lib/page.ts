import { Metadata } from "next";
import { fetchCmsContent } from "./cms";

type Page = {
    createdAt: string,
    slug: string,
    title: string,
    overview: string,
    body: {
        json: any
    }
}

type CmsListPagesResponse = {
    data: {
        pages: {
            createdAt: string,
            slug: string,
            title: string,
        }[]
    }
};

type CmsPageMetaResponse = {
    data: {
        page: {
            metadata: {
                title: string,
                description: string,
                image: {
                    width: number,
                    height: number,
                    url: string
                }
            }
        }
    }
};

type CmsGetPageResponse = {
    data: {
        page: Page
    }
};

type CmsMenuItem = {
    text: string,
    link: string,
}

export async function getPage(slug: string): Promise<Page | null> {
    try {

        const result = await fetchCmsContent<CmsGetPageResponse>(
            `query getPage($slug:String!) {
                page(where:{slug:$slug}) {
                    createdAt,
                    slug,
                    title,
                    overview,
                    body {
                        json
                    }
                }
            }`,
            { slug });

        if (!result) {
            return null;
        }

        return result.data.page;
    }
    catch (err) {
        console.error("Failed to get page", err, slug);
    }

    return null;
}

export async function buildPageMenu(): Promise<CmsMenuItem[]> {

    try {

        const result = await fetchCmsContent<CmsListPagesResponse>('query Pages { pages { createdAt, slug, title } }');
        if (!result) {
            return [];
        }

        return result.data.pages.map(page => {
            return {
                text: page.title,
                link: `/${page.slug}`
            };
        });
    }
    catch (err) {
        console.error("Failed to build page menu", err);
    }

    return [];
}

export async function getPageMetadata(slug: string): Promise<Metadata> {

    try {
        const result = await fetchCmsContent<CmsPageMetaResponse>(
            `query getPageMeta($slug:String!) {
                page(where:{slug:$slug}) {
                    metadata {
                        title,
                        description,
                        image {
                            width,
                            height,
                            url
                        }
                    }
                }
            }`,
            { slug });

        if (!result) {
            return {};
        }

        return {
            title: result.data?.page?.metadata.title,
            description: result.data?.page?.metadata.description,
            openGraph: {
                images: [result.data?.page?.metadata.image?.url],
                title: result.data?.page?.metadata.title,
                description: result.data?.page?.metadata.description,
            },
        }
    }
    catch (e) {
        console.error("Failed to get page metadata", e, slug);
    }

    return {};
}