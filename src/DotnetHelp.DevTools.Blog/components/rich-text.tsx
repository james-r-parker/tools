import { RichText, RichTextProps } from "@graphcms/rich-text-react-renderer";
import Link from "next/link";
import Image from "next/image";

export default async function CmsContentRender({ options }: { options: RichTextProps }) {
    return (
        <RichText
            {...options}
            renderers={{
                h1: ({ children }) => <h1 className="text-white text-3xl font-semibold my-4">{children}</h1>,
                h2: ({ children }) => <h1 className="text-white text-2xl font-semibold my-4">{children}</h1>,
                h3: ({ children }) => <h1 className="text-white text-xl font-semibold my-4">{children}</h1>,
                h4: ({ children }) => <h1 className="text-white text-xl">{children}</h1>,
                h5: ({ children }) => <h1 className="text-white text-lg">{children}</h1>,
                h6: ({ children }) => <h1 className="text-white">{children}</h1>,
                bold: ({ children }) => <span className="font-bold">{children}</span>,
                ul: ({ children }) => <ul className="list-disc list-inside my-4">{children}</ul>,
                ol: ({ children }) => <ol className="list-decimal list-inside my-4">{children}</ol>,
                a: ({ children, href, title }) => <Link className="text-sky-500 dark:text-sky-400 hover:underline" href={href!} title={title}>{children}</Link>,
                underline: ({ children }) => <span className="underline decoration-solid">{children}</span>,
                img: ({ src, width, height, altText }) => <Image src={src!} alt={altText || ""} width={width} height={height} className="my-2" />,
                code_block: ({ children }) => <pre className="bg-gray-800 p-4 rounded-md my-8"><code className="language-javascript">{children}</code></pre>,
                table: ({ children }) => <table className="table-auto w-full my-4">{children}</table>,
                table_header_cell: ({ children }) => <th className="border-b dark:border-slate-600 font-medium p-4 pl-8 pt-0 pb-3 text-slate-400 dark:text-slate-200 text-left">{children}</th>,
                table_body: ({ children }) => <tbody className="bg-white dark:bg-slate-800">{children}</tbody>,
                table_cell: ({ children }) => <td className="border-b border-slate-100 dark:border-slate-700 p-4 pl-8 text-slate-500 dark:text-slate-400">{children}</td>,
            }}
        />
    );
}